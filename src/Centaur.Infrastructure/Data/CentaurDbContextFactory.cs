using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Centaur.Infrastructure.Data;

public class CentaurDbContextFactory : IDesignTimeDbContextFactory<CentaurDbContext>
{
    public CentaurDbContext CreateDbContext(string[] args)
    {
        // Design-time connection string voor EF migraties
        // In productie wordt de connection string via appsettings geconfigureerd
        var connectionString = "Host=localhost;Port=5433;Database=centaur;Username=postgres;Password=postgres";

        var optionsBuilder = new DbContextOptionsBuilder<CentaurDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CentaurDbContext(optionsBuilder.Options);
    }
}
