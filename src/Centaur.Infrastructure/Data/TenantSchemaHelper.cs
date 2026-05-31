// src/Centaur.Infrastructure/Data/TenantSchemaHelper.cs
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Data;

public class TenantSchemaHelper(CentaurDbContext context)
{
    public async Task CreateTenantSchemaAsync(string slug)
    {
        var schemaName = SlugToSchema(slug);
        await context.Database.ExecuteSqlRawAsync($$"""
            CREATE SCHEMA IF NOT EXISTS "{{schemaName}}";
            CREATE TABLE IF NOT EXISTS "{{schemaName}}".content_types (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(255) NOT NULL,
                slug VARCHAR(100) NOT NULL UNIQUE,
                fields JSONB NOT NULL DEFAULT '[]',
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS "{{schemaName}}".entries (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                content_type_id UUID NOT NULL REFERENCES "{{schemaName}}".content_types(id) ON DELETE CASCADE,
                data JSONB NOT NULL DEFAULT '{}',
                status VARCHAR(20) NOT NULL DEFAULT 'draft',
                published_at TIMESTAMPTZ,
                created_by UUID,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS "{{schemaName}}".media (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                filename VARCHAR(500) NOT NULL,
                url VARCHAR(1000) NOT NULL,
                mime_type VARCHAR(100),
                size BIGINT,
                alt VARCHAR(500),
                created_by UUID,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS "{{schemaName}}".settings (
                key VARCHAR(255) PRIMARY KEY,
                value JSONB NOT NULL
            );
            """);
    }

    public static string SlugToSchema(string slug) =>
        slug.Replace("-", "_").ToLowerInvariant();
}
