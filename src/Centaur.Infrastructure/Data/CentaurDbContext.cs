// src/Centaur.Infrastructure/Data/CentaurDbContext.cs
using Centaur.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Data;

public class CentaurDbContext(DbContextOptions<CentaurDbContext> options) : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<User> Users => Set<User>();
    public DbSet<ApiKey> ApiKeys => Set<ApiKey>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<Tenant>(e =>
        {
            e.ToTable("tenants");
            e.HasKey(t => t.Id);
            e.Property(t => t.Slug).HasMaxLength(100).IsRequired();
            e.HasIndex(t => t.Slug).IsUnique();
            e.Property(t => t.Name).HasMaxLength(255).IsRequired();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).HasMaxLength(255).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Role).HasConversion<string>();
            e.HasOne(u => u.Tenant).WithMany(t => t.Users).HasForeignKey(u => u.TenantId).IsRequired(false);
        });

        modelBuilder.Entity<ApiKey>(e =>
        {
            e.ToTable("api_keys");
            e.HasKey(k => k.Id);
            e.HasOne(k => k.Tenant).WithMany(t => t.ApiKeys).HasForeignKey(k => k.TenantId);
        });
    }
}
