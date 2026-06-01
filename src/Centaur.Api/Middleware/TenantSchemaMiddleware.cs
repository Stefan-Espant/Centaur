using Centaur.Application.Interfaces;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Api.Middleware;

public class TenantSchemaMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, CentaurDbContext db, ITenantRepository tenantRepository)
    {
        var tenantIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "tenant_id");

        if (tenantIdClaim is not null
            && !string.IsNullOrEmpty(tenantIdClaim.Value)
            && Guid.TryParse(tenantIdClaim.Value, out var tenantId))
        {
            var tenant = await tenantRepository.GetByIdAsync(tenantId);
            if (tenant is not null)
            {
                var schema = TenantSchemaHelper.SlugToSchema(tenant.Slug);
                context.Items["TenantSchema"] = schema;
                await db.Database.ExecuteSqlRawAsync($"SET search_path TO \"{schema}\", public");
            }
        }

        await next(context);
    }
}
