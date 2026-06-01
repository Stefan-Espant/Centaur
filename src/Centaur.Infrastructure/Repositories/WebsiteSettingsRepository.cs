using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class WebsiteSettingsRepository(CentaurDbContext context) : IWebsiteSettingsRepository
{
    private const string SettingsKey = "website";
    private static readonly Regex SchemaNamePattern = new(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<WebsiteSettingsDto?> GetAsync(string tenantSchema)
    {
        EnsureValidSchema(tenantSchema);

        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT value::text FROM \"{tenantSchema}\".settings WHERE key = @key";

            var keyParam = command.CreateParameter();
            keyParam.ParameterName = "key";
            keyParam.Value = SettingsKey;
            command.Parameters.Add(keyParam);

            var result = await command.ExecuteScalarAsync();
            return result is string json
                ? JsonSerializer.Deserialize<WebsiteSettingsDto>(json, JsonOptions)
                : null;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public async Task<WebsiteSettingsDto> UpsertAsync(string tenantSchema, WebsiteSettingsDto settings)
    {
        EnsureValidSchema(tenantSchema);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        var connection = context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync();

        try
        {
            await using var command = connection.CreateCommand();
            command.CommandText =
                $"INSERT INTO \"{tenantSchema}\".settings (key, value) " +
                "VALUES (@key, CAST(@value AS jsonb)) " +
                "ON CONFLICT (key) DO UPDATE SET value = EXCLUDED.value";

            var keyParam = command.CreateParameter();
            keyParam.ParameterName = "key";
            keyParam.Value = SettingsKey;
            command.Parameters.Add(keyParam);

            var valueParam = command.CreateParameter();
            valueParam.ParameterName = "value";
            valueParam.Value = json;
            command.Parameters.Add(valueParam);

            await command.ExecuteNonQueryAsync();
            return settings;
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private static void EnsureValidSchema(string tenantSchema)
    {
        if (!SchemaNamePattern.IsMatch(tenantSchema))
            throw new ArgumentException($"Ongeldige schema naam: {tenantSchema}");
    }
}
