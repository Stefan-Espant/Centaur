// src/Centaur.Infrastructure/Data/TenantSchemaHelper.cs
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Data;

public class TenantSchemaHelper(CentaurDbContext context)
{
    public async Task CreateTenantSchemaAsync(string slug)
    {
        var schemaName = SlugToSchema(slug);
        if (!System.Text.RegularExpressions.Regex.IsMatch(schemaName, @"^[a-z][a-z0-9_]*$"))
            throw new ArgumentException($"Ongeldige schema naam: {schemaName}");

        // EF Core's ExecuteSqlRawAsync uses String.Format internally so curly braces must be escaped
        var sql =
            $"CREATE SCHEMA IF NOT EXISTS \"{schemaName}\";\n" +
            $"CREATE TABLE IF NOT EXISTS \"{schemaName}\".content_types (\n" +
            "    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),\n" +
            "    name VARCHAR(255) NOT NULL,\n" +
            "    slug VARCHAR(100) NOT NULL UNIQUE,\n" +
            "    fields JSONB NOT NULL DEFAULT '[]',\n" +
            "    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()\n" +
            ");\n" +
            $"CREATE TABLE IF NOT EXISTS \"{schemaName}\".entries (\n" +
            "    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),\n" +
            $"    content_type_id UUID NOT NULL REFERENCES \"{schemaName}\".content_types(id) ON DELETE CASCADE,\n" +
            "    data JSONB NOT NULL DEFAULT '{{}}'::jsonb,\n" +
            "    status VARCHAR(20) NOT NULL DEFAULT 'draft',\n" +
            "    published_at TIMESTAMPTZ,\n" +
            "    created_by UUID,\n" +
            "    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),\n" +
            "    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()\n" +
            ");\n" +
            $"CREATE TABLE IF NOT EXISTS \"{schemaName}\".media (\n" +
            "    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),\n" +
            "    filename VARCHAR(500) NOT NULL,\n" +
            "    url VARCHAR(1000) NOT NULL,\n" +
            "    mime_type VARCHAR(100),\n" +
            "    size BIGINT,\n" +
            "    alt VARCHAR(500),\n" +
            "    created_by UUID,\n" +
            "    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()\n" +
            ");\n" +
            $"CREATE TABLE IF NOT EXISTS \"{schemaName}\".settings (\n" +
            "    key VARCHAR(255) PRIMARY KEY,\n" +
            "    value JSONB NOT NULL\n" +
            ");";
        await context.Database.ExecuteSqlRawAsync(sql);
    }

    public static string SlugToSchema(string slug) =>
        slug.Replace("-", "_").ToLowerInvariant();
}
