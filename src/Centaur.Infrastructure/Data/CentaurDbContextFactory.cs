using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Centaur.Infrastructure.Data;

public class CentaurDbContextFactory : IDesignTimeDbContextFactory<CentaurDbContext>
{
    public CentaurDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? throw new InvalidOperationException("Stel de omgevingsvariabele ConnectionStrings__Default in voor EF-migraties.");

        var optionsBuilder = new DbContextOptionsBuilder<CentaurDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CentaurDbContext(optionsBuilder.Options);
    }
}
