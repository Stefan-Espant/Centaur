using System.Data;
using System.Text.Json;
using System.Text.RegularExpressions;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class PageRepository(CentaurDbContext context) : IPageRepository
{
    private const string SettingsKey = "pages";
    private static readonly Regex SchemaNamePattern = new(@"^[a-z][a-z0-9_]*$", RegexOptions.Compiled);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<PageDto>> GetAllAsync(string tenantSchema)
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
            var pages = result is string json
                ? JsonSerializer.Deserialize<List<PageDto>>(json, JsonOptions) ?? []
                : [];

            return pages
                .OrderBy(p => p.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
    }

    public async Task<PageDto?> GetByIdAsync(string tenantSchema, Guid id) =>
        (await GetAllAsync(tenantSchema)).FirstOrDefault(page => page.Id == id);

    public async Task<PageDto> CreateAsync(string tenantSchema, PageDto page)
    {
        var pages = (await GetAllAsync(tenantSchema)).ToList();
        pages.Add(page);
        await SaveAsync(tenantSchema, pages);
        return page;
    }

    public async Task<PageDto> UpdateAsync(string tenantSchema, PageDto page)
    {
        var pages = (await GetAllAsync(tenantSchema)).ToList();
        var index = pages.FindIndex(existing => existing.Id == page.Id);
        if (index < 0) throw new KeyNotFoundException("Pagina niet gevonden.");

        pages[index] = page;
        await SaveAsync(tenantSchema, pages);
        return page;
    }

    public async Task DeleteAsync(string tenantSchema, Guid id)
    {
        var pages = (await GetAllAsync(tenantSchema)).ToList();
        var removed = pages.RemoveAll(page => page.Id == id);
        if (removed == 0) return;
        await SaveAsync(tenantSchema, pages);
    }

    private async Task SaveAsync(string tenantSchema, List<PageDto> pages)
    {
        EnsureValidSchema(tenantSchema);

        var json = JsonSerializer.Serialize(pages, JsonOptions);
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
