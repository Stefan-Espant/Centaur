using System.Data;
using System.Data.Common;
using System.Text.Json;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.ValueObjects;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class BlockTypeRepository(CentaurDbContext context, ITenantSchemaAccessor tenantSchemaAccessor) : IBlockTypeRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<List<BlockType>> GetAllAsync()
    {
        var result = new List<BlockType>();
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await EnsureTableAsync(connection);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"""
                SELECT id, name, slug, fields::text, created_at, updated_at
                FROM {QualifyTableName()}
                ORDER BY name
                """;

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(Map(reader));
            }
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }

        return result;
    }

    public async Task<BlockType?> GetBySlugAsync(string slug)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await EnsureTableAsync(connection);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"""
                SELECT id, name, slug, fields::text, created_at, updated_at
                FROM {QualifyTableName()}
                WHERE slug = @slug
                LIMIT 1
                """;

            var param = command.CreateParameter();
            param.ParameterName = "slug";
            param.Value = slug;
            command.Parameters.Add(param);

            await using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? Map(reader) : null;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public async Task<BlockType> CreateAsync(BlockType blockType)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await EnsureTableAsync(connection);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"""
                INSERT INTO {QualifyTableName()} (id, name, slug, fields, created_at, updated_at)
                VALUES (@id, @name, @slug, CAST(@fields AS jsonb), @createdAt, @updatedAt)
                """;

            AddParam(command, "id", blockType.Id);
            AddParam(command, "name", blockType.Name);
            AddParam(command, "slug", blockType.Slug);
            AddParam(command, "fields", JsonSerializer.Serialize(blockType.Fields, JsonOptions));
            AddParam(command, "createdAt", blockType.CreatedAt);
            AddParam(command, "updatedAt", blockType.UpdatedAt);

            await command.ExecuteNonQueryAsync();
            return blockType;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public async Task<BlockType> UpdateAsync(BlockType blockType)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await EnsureTableAsync(connection);
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"""
                UPDATE {QualifyTableName()}
                SET name = @name,
                    fields = CAST(@fields AS jsonb),
                    updated_at = @updatedAt
                WHERE id = @id
                """;

            AddParam(command, "id", blockType.Id);
            AddParam(command, "name", blockType.Name);
            AddParam(command, "fields", JsonSerializer.Serialize(blockType.Fields, JsonOptions));
            AddParam(command, "updatedAt", blockType.UpdatedAt);

            await command.ExecuteNonQueryAsync();
            return blockType;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public async Task DeleteAsync(string slug)
    {
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await EnsureTableAsync(connection);
            await using var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM {QualifyTableName()} WHERE slug = @slug";
            AddParam(command, "slug", slug);
            await command.ExecuteNonQueryAsync();
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private static BlockType Map(IDataRecord record) => new()
    {
        Id = record.GetGuid(0),
        Name = record.GetString(1),
        Slug = record.GetString(2),
        Fields = JsonSerializer.Deserialize<List<FieldDefinition>>(record.GetString(3), JsonOptions) ?? [],
        CreatedAt = record.GetDateTime(4),
        UpdatedAt = record.GetDateTime(5)
    };

    private static void AddParam(IDbCommand command, string name, object? value)
    {
        var param = command.CreateParameter();
        param.ParameterName = name;
        param.Value = value ?? DBNull.Value;
        command.Parameters.Add(param);
    }

    private async Task EnsureTableAsync(DbConnection connection)
    {
        await using var command = connection.CreateCommand();
        command.CommandText =
            $"""
            CREATE TABLE IF NOT EXISTS {QualifyTableName()} (
                id UUID PRIMARY KEY,
                name VARCHAR(255) NOT NULL,
                slug VARCHAR(255) NOT NULL UNIQUE,
                fields JSONB NOT NULL DEFAULT '[]',
                created_at TIMESTAMPTZ NOT NULL DEFAULT now(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT now()
            )
            """;
        await command.ExecuteNonQueryAsync();
    }

    private string QualifyTableName()
    {
        var schema = tenantSchemaAccessor.CurrentSchema;
        return string.IsNullOrWhiteSpace(schema) ? "\"public\".block_types" : $"\"{schema}\".block_types";
    }
}
