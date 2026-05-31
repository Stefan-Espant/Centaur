# Centaur CMS — Plan 1: Foundation & Authenticatie

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Opzetten van de volledige .NET Clean Architecture solution, PostgreSQL schema-per-tenant, JWT-authenticatie en tenant-beheer — zodat Plan 2 (Content API) direct bovenop kan worden gebouwd.

**Architecture:** ASP.NET Core Web API met vier projecten (Domain, Application, Infrastructure, Api). PostgreSQL met een `public` schema voor gedeelde data (tenants, users) en een apart schema per tenant. JWT-tokens bevatten `tenant_id`, `user_id` en `role`. Elke API-aanroep selecteert het juiste PostgreSQL-schema via middleware.

**Tech Stack:** .NET 9, ASP.NET Core Web API, Entity Framework Core 9, Npgsql, BCrypt.Net, System.IdentityModel.Tokens.Jwt, xUnit, PostgreSQL 16

---

## Bestandsstructuur

```
Centaur/
├── Centaur.sln
├── src/
│   ├── Centaur.Domain/
│   │   ├── Centaur.Domain.csproj
│   │   ├── Entities/
│   │   │   ├── Tenant.cs
│   │   │   ├── User.cs
│   │   │   └── ApiKey.cs
│   │   └── Enums/
│   │       └── UserRole.cs
│   ├── Centaur.Application/
│   │   ├── Centaur.Application.csproj
│   │   ├── Interfaces/
│   │   │   ├── ITenantRepository.cs
│   │   │   └── IUserRepository.cs
│   │   ├── DTOs/
│   │   │   ├── TenantDto.cs
│   │   │   ├── CreateTenantRequest.cs
│   │   │   ├── LoginRequest.cs
│   │   │   └── AuthResponse.cs
│   │   └── Services/
│   │       ├── TenantService.cs
│   │       └── AuthService.cs
│   ├── Centaur.Infrastructure/
│   │   ├── Centaur.Infrastructure.csproj
│   │   ├── Data/
│   │   │   ├── CentaurDbContext.cs
│   │   │   └── TenantSchemaHelper.cs
│   │   └── Repositories/
│   │       ├── TenantRepository.cs
│   │       └── UserRepository.cs
│   └── Centaur.Api/
│       ├── Centaur.Api.csproj
│       ├── Program.cs
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Controllers/
│       │   ├── AuthController.cs
│       │   └── TenantsController.cs
│       └── Middleware/
│           └── TenantSchemaMiddleware.cs
└── tests/
    ├── Centaur.Application.Tests/
    │   ├── Centaur.Application.Tests.csproj
    │   ├── Services/
    │   │   ├── TenantServiceTests.cs
    │   │   └── AuthServiceTests.cs
    │   └── Helpers/
    │       └── MockRepository.cs
    └── Centaur.Api.Tests/
        ├── Centaur.Api.Tests.csproj
        └── Controllers/
            ├── AuthControllerTests.cs
            └── TenantsControllerTests.cs
```

---

## Task 1: .NET Solution aanmaken

**Files:**
- Maak: `Centaur.sln`
- Maak: `src/Centaur.Domain/Centaur.Domain.csproj`
- Maak: `src/Centaur.Application/Centaur.Application.csproj`
- Maak: `src/Centaur.Infrastructure/Centaur.Infrastructure.csproj`
- Maak: `src/Centaur.Api/Centaur.Api.csproj`
- Maak: `tests/Centaur.Application.Tests/Centaur.Application.Tests.csproj`
- Maak: `tests/Centaur.Api.Tests/Centaur.Api.Tests.csproj`

- [ ] **Stap 1: Maak de solution en projecten aan**

```bash
dotnet new sln -n Centaur
dotnet new classlib -n Centaur.Domain -o src/Centaur.Domain
dotnet new classlib -n Centaur.Application -o src/Centaur.Application
dotnet new classlib -n Centaur.Infrastructure -o src/Centaur.Infrastructure
dotnet new webapi -n Centaur.Api -o src/Centaur.Api --no-openapi false
dotnet new xunit -n Centaur.Application.Tests -o tests/Centaur.Application.Tests
dotnet new xunit -n Centaur.Api.Tests -o tests/Centaur.Api.Tests
```

- [ ] **Stap 2: Voeg alle projecten toe aan de solution**

```bash
dotnet sln add src/Centaur.Domain/Centaur.Domain.csproj
dotnet sln add src/Centaur.Application/Centaur.Application.csproj
dotnet sln add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj
dotnet sln add src/Centaur.Api/Centaur.Api.csproj
dotnet sln add tests/Centaur.Application.Tests/Centaur.Application.Tests.csproj
dotnet sln add tests/Centaur.Api.Tests/Centaur.Api.Tests.csproj
```

- [ ] **Stap 3: Voeg projectreferenties toe**

```bash
dotnet add src/Centaur.Application/Centaur.Application.csproj reference src/Centaur.Domain/Centaur.Domain.csproj
dotnet add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj reference src/Centaur.Application/Centaur.Application.csproj
dotnet add src/Centaur.Api/Centaur.Api.csproj reference src/Centaur.Application/Centaur.Application.csproj
dotnet add src/Centaur.Api/Centaur.Api.csproj reference src/Centaur.Infrastructure/Centaur.Infrastructure.csproj
dotnet add tests/Centaur.Application.Tests/Centaur.Application.Tests.csproj reference src/Centaur.Application/Centaur.Application.csproj
dotnet add tests/Centaur.Api.Tests/Centaur.Api.Tests.csproj reference src/Centaur.Api/Centaur.Api.csproj
```

- [ ] **Stap 4: Installeer NuGet-pakketten**

```bash
# Infrastructure
dotnet add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj package Microsoft.EntityFrameworkCore
dotnet add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj package BCrypt.Net-Next

# Api
dotnet add src/Centaur.Api/Centaur.Api.csproj package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add src/Centaur.Api/Centaur.Api.csproj package Swashbuckle.AspNetCore

# Tests
dotnet add tests/Centaur.Application.Tests/Centaur.Application.Tests.csproj package Moq
dotnet add tests/Centaur.Api.Tests/Centaur.Api.Tests.csproj package Microsoft.AspNetCore.Mvc.Testing
dotnet add tests/Centaur.Api.Tests/Centaur.Api.Tests.csproj package Moq
```

- [ ] **Stap 5: Verifieer dat de solution bouwt**

```bash
dotnet build
```

Verwacht: `Build succeeded.`

- [ ] **Stap 6: Commit**

```bash
git add .
git commit -m "feat: initialiseer .NET Clean Architecture solution"
```

---

## Task 2: Domain Entiteiten

**Files:**
- Maak: `src/Centaur.Domain/Enums/UserRole.cs`
- Maak: `src/Centaur.Domain/Entities/Tenant.cs`
- Maak: `src/Centaur.Domain/Entities/User.cs`
- Maak: `src/Centaur.Domain/Entities/ApiKey.cs`

- [ ] **Stap 1: Verwijder de gegenereerde Class1.cs**

```bash
rm src/Centaur.Domain/Class1.cs
rm src/Centaur.Application/Class1.cs
rm src/Centaur.Infrastructure/Class1.cs
```

- [ ] **Stap 2: Maak `UserRole.cs`**

```csharp
// src/Centaur.Domain/Enums/UserRole.cs
namespace Centaur.Domain.Enums;

public enum UserRole
{
    SuperAdmin,
    Admin,
    Editor,
    Viewer
}
```

- [ ] **Stap 3: Maak `Tenant.cs`**

```csharp
// src/Centaur.Domain/Entities/Tenant.cs
namespace Centaur.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
```

- [ ] **Stap 4: Maak `User.cs`**

```csharp
// src/Centaur.Domain/Entities/User.cs
using Centaur.Domain.Enums;

namespace Centaur.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public Guid? TenantId { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
```

- [ ] **Stap 5: Maak `ApiKey.cs`**

```csharp
// src/Centaur.Domain/Entities/ApiKey.cs
namespace Centaur.Domain.Entities;

public class ApiKey
{
    public Guid Id { get; set; }
    public string KeyHash { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Tenant? Tenant { get; set; }
}
```

- [ ] **Stap 6: Build ter verificatie**

```bash
dotnet build src/Centaur.Domain/Centaur.Domain.csproj
```

Verwacht: `Build succeeded.`

- [ ] **Stap 7: Commit**

```bash
git add src/Centaur.Domain/
git commit -m "feat: voeg domain entiteiten toe (Tenant, User, ApiKey)"
```

---

## Task 3: Application Interfaces & DTOs

**Files:**
- Maak: `src/Centaur.Application/Interfaces/ITenantRepository.cs`
- Maak: `src/Centaur.Application/Interfaces/IUserRepository.cs`
- Maak: `src/Centaur.Application/DTOs/TenantDto.cs`
- Maak: `src/Centaur.Application/DTOs/CreateTenantRequest.cs`
- Maak: `src/Centaur.Application/DTOs/LoginRequest.cs`
- Maak: `src/Centaur.Application/DTOs/AuthResponse.cs`

- [ ] **Stap 1: Maak `ITenantRepository.cs`**

```csharp
// src/Centaur.Application/Interfaces/ITenantRepository.cs
using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> CreateAsync(Tenant tenant);
    Task DeleteAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug);
}
```

- [ ] **Stap 2: Maak `IUserRepository.cs`**

```csharp
// src/Centaur.Application/Interfaces/IUserRepository.cs
using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByIdAsync(Guid id);
    Task<User> CreateAsync(User user);
}
```

- [ ] **Stap 3: Maak de DTOs**

```csharp
// src/Centaur.Application/DTOs/TenantDto.cs
namespace Centaur.Application.DTOs;

public record TenantDto(Guid Id, string Slug, string Name, DateTime CreatedAt);
```

```csharp
// src/Centaur.Application/DTOs/CreateTenantRequest.cs
namespace Centaur.Application.DTOs;

public record CreateTenantRequest(string Name, string Slug, string AdminEmail, string AdminPassword);
```

```csharp
// src/Centaur.Application/DTOs/LoginRequest.cs
namespace Centaur.Application.DTOs;

public record LoginRequest(string Email, string Password);
```

```csharp
// src/Centaur.Application/DTOs/AuthResponse.cs
namespace Centaur.Application.DTOs;

public record AuthResponse(string AccessToken, string RefreshToken, string Role, Guid? TenantId);
```

- [ ] **Stap 4: Build ter verificatie**

```bash
dotnet build src/Centaur.Application/Centaur.Application.csproj
```

Verwacht: `Build succeeded.`

- [ ] **Stap 5: Commit**

```bash
git add src/Centaur.Application/
git commit -m "feat: voeg application interfaces en DTOs toe"
```

---

## Task 4: AuthService (met tests)

**Files:**
- Maak: `src/Centaur.Application/Services/AuthService.cs`
- Maak: `tests/Centaur.Application.Tests/Services/AuthServiceTests.cs`
- Maak: `tests/Centaur.Application.Tests/Helpers/MockUserRepository.cs`

- [ ] **Stap 1: Schrijf de falende test**

```csharp
// tests/Centaur.Application.Tests/Helpers/MockUserRepository.cs
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;

namespace Centaur.Application.Tests.Helpers;

public class MockUserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public void Seed(User user) => _users.Add(user);

    public Task<User?> GetByEmailAsync(string email) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Email == email));

    public Task<User?> GetByIdAsync(Guid id) =>
        Task.FromResult(_users.FirstOrDefault(u => u.Id == id));

    public Task<User> CreateAsync(User user)
    {
        _users.Add(user);
        return Task.FromResult(user);
    }
}
```

```csharp
// tests/Centaur.Application.Tests/Services/AuthServiceTests.cs
using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Xunit;

namespace Centaur.Application.Tests.Services;

public class AuthServiceTests
{
    private readonly MockUserRepository _repo = new();
    private const string JwtSecret = "test-secret-key-that-is-long-enough-32chars";

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokens()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.nl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("wachtwoord123"),
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _repo.Seed(user);

        var service = new AuthService(_repo, JwtSecret);
        var result = await service.LoginAsync(new LoginRequest("admin@test.nl", "wachtwoord123"));

        Assert.NotNull(result);
        Assert.NotEmpty(result.AccessToken);
        Assert.Equal("Admin", result.Role);
    }

    [Fact]
    public async Task Login_WrongPassword_ReturnsNull()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@test.nl",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("juistewachtwoord"),
            Role = UserRole.Admin,
            TenantId = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };
        _repo.Seed(user);

        var service = new AuthService(_repo, JwtSecret);
        var result = await service.LoginAsync(new LoginRequest("admin@test.nl", "foutewachtwoord"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_UnknownEmail_ReturnsNull()
    {
        var service = new AuthService(_repo, JwtSecret);
        var result = await service.LoginAsync(new LoginRequest("onbekend@test.nl", "wachtwoord"));

        Assert.Null(result);
    }
}
```

- [ ] **Stap 2: Voer de test uit en verifieer dat ze falen**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "AuthServiceTests"
```

Verwacht: fout met `type or namespace 'AuthService' could not be found`

- [ ] **Stap 3: Installeer BCrypt in Application.Tests**

```bash
dotnet add tests/Centaur.Application.Tests/Centaur.Application.Tests.csproj package BCrypt.Net-Next
```

- [ ] **Stap 4: Schrijf de minimale implementatie**

```csharp
// src/Centaur.Application/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Centaur.Application.Services;

public class AuthService(IUserRepository userRepository, string jwtSecret)
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var accessToken = GenerateToken(user, TimeSpan.FromMinutes(15));
        var refreshToken = GenerateToken(user, TimeSpan.FromDays(7));

        return new AuthResponse(accessToken, refreshToken, user.Role.ToString(), user.TenantId);
    }

    private string GenerateToken(Domain.Entities.User user, TimeSpan expiry)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("tenant_id", user.TenantId?.ToString() ?? string.Empty)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

- [ ] **Stap 5: Voer de tests uit en verifieer dat ze slagen**

```bash
dotnet add src/Centaur.Application/Centaur.Application.csproj package Microsoft.IdentityModel.Tokens
dotnet add src/Centaur.Application/Centaur.Application.csproj package System.IdentityModel.Tokens.Jwt
dotnet add src/Centaur.Application/Centaur.Application.csproj package BCrypt.Net-Next
dotnet test tests/Centaur.Application.Tests/ --filter "AuthServiceTests" -v normal
```

Verwacht: `3 passed`

- [ ] **Stap 6: Commit**

```bash
git add src/Centaur.Application/Services/AuthService.cs tests/Centaur.Application.Tests/
git commit -m "feat: voeg AuthService toe met JWT-tokenlogica en tests"
```

---

## Task 5: TenantService (met tests)

**Files:**
- Maak: `src/Centaur.Application/Services/TenantService.cs`
- Maak: `tests/Centaur.Application.Tests/Services/TenantServiceTests.cs`
- Maak: `tests/Centaur.Application.Tests/Helpers/MockTenantRepository.cs`

- [ ] **Stap 1: Schrijf de falende tests**

```csharp
// tests/Centaur.Application.Tests/Helpers/MockTenantRepository.cs
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Tests.Helpers;

public class MockTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _tenants = new();

    public void Seed(Tenant tenant) => _tenants.Add(tenant);

    public Task<Tenant?> GetByIdAsync(Guid id) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Id == id));

    public Task<Tenant?> GetBySlugAsync(string slug) =>
        Task.FromResult(_tenants.FirstOrDefault(t => t.Slug == slug));

    public Task<IEnumerable<Tenant>> GetAllAsync() =>
        Task.FromResult<IEnumerable<Tenant>>(_tenants);

    public Task<Tenant> CreateAsync(Tenant tenant)
    {
        _tenants.Add(tenant);
        return Task.FromResult(tenant);
    }

    public Task DeleteAsync(Guid id)
    {
        _tenants.RemoveAll(t => t.Id == id);
        return Task.CompletedTask;
    }

    public Task<bool> SlugExistsAsync(string slug) =>
        Task.FromResult(_tenants.Any(t => t.Slug == slug));
}
```

```csharp
// tests/Centaur.Application.Tests/Services/TenantServiceTests.cs
using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
using Centaur.Domain.Entities;
using Xunit;

namespace Centaur.Application.Tests.Services;

public class TenantServiceTests
{
    private readonly MockTenantRepository _tenantRepo = new();
    private readonly MockUserRepository _userRepo = new();

    [Fact]
    public async Task CreateTenant_NewSlug_ReturnsTenantDto()
    {
        var service = new TenantService(_tenantRepo, _userRepo);
        var request = new CreateTenantRequest("Bakkerij De Molen", "de-molen", "admin@demolen.nl", "Wachtwoord1!");

        var result = await service.CreateAsync(request);

        Assert.NotNull(result);
        Assert.Equal("de-molen", result.Slug);
        Assert.Equal("Bakkerij De Molen", result.Name);
    }

    [Fact]
    public async Task CreateTenant_DuplicateSlug_ThrowsException()
    {
        _tenantRepo.Seed(new Tenant { Id = Guid.NewGuid(), Slug = "de-molen", Name = "Bestaande bakkerij", CreatedAt = DateTime.UtcNow });
        var service = new TenantService(_tenantRepo, _userRepo);
        var request = new CreateTenantRequest("Nieuwe Bakkerij", "de-molen", "nieuw@bakkerij.nl", "Wachtwoord1!");

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.CreateAsync(request));
    }

    [Fact]
    public async Task GetAll_ReturnsMappedDtos()
    {
        _tenantRepo.Seed(new Tenant { Id = Guid.NewGuid(), Slug = "tenant-a", Name = "Tenant A", CreatedAt = DateTime.UtcNow });
        _tenantRepo.Seed(new Tenant { Id = Guid.NewGuid(), Slug = "tenant-b", Name = "Tenant B", CreatedAt = DateTime.UtcNow });
        var service = new TenantService(_tenantRepo, _userRepo);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count());
    }
}
```

- [ ] **Stap 2: Voer de tests uit en verifieer dat ze falen**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "TenantServiceTests"
```

Verwacht: `type or namespace 'TenantService' could not be found`

- [ ] **Stap 3: Schrijf de minimale implementatie**

```csharp
// src/Centaur.Application/Services/TenantService.cs
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;

namespace Centaur.Application.Services;

public class TenantService(ITenantRepository tenantRepository, IUserRepository userRepository)
{
    public async Task<TenantDto> CreateAsync(CreateTenantRequest request)
    {
        if (await tenantRepository.SlugExistsAsync(request.Slug))
            throw new InvalidOperationException($"Slug '{request.Slug}' is al in gebruik.");

        var tenant = await tenantRepository.CreateAsync(new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = request.Slug,
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        });

        await userRepository.CreateAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = request.AdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            Role = UserRole.Admin,
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow
        });

        return new TenantDto(tenant.Id, tenant.Slug, tenant.Name, tenant.CreatedAt);
    }

    public async Task<IEnumerable<TenantDto>> GetAllAsync()
    {
        var tenants = await tenantRepository.GetAllAsync();
        return tenants.Select(t => new TenantDto(t.Id, t.Slug, t.Name, t.CreatedAt));
    }

    public async Task DeleteAsync(Guid id) => await tenantRepository.DeleteAsync(id);
}
```

- [ ] **Stap 4: Voer de tests uit en verifieer dat ze slagen**

```bash
dotnet test tests/Centaur.Application.Tests/ -v normal
```

Verwacht: `6 passed` (3 AuthService + 3 TenantService)

- [ ] **Stap 5: Commit**

```bash
git add src/Centaur.Application/Services/TenantService.cs tests/Centaur.Application.Tests/
git commit -m "feat: voeg TenantService toe met tests"
```

---

## Task 6: Infrastructure — DbContext & Repositories

**Files:**
- Maak: `src/Centaur.Infrastructure/Data/CentaurDbContext.cs`
- Maak: `src/Centaur.Infrastructure/Data/TenantSchemaHelper.cs`
- Maak: `src/Centaur.Infrastructure/Repositories/TenantRepository.cs`
- Maak: `src/Centaur.Infrastructure/Repositories/UserRepository.cs`

- [ ] **Stap 1: Maak `CentaurDbContext.cs`**

```csharp
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
```

- [ ] **Stap 2: Maak `TenantSchemaHelper.cs`**

```csharp
// src/Centaur.Infrastructure/Data/TenantSchemaHelper.cs
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Data;

public class TenantSchemaHelper(CentaurDbContext context)
{
    public async Task CreateTenantSchemaAsync(string slug)
    {
        var schemaName = SlugToSchema(slug);
        await context.Database.ExecuteSqlRawAsync($"""
            CREATE SCHEMA IF NOT EXISTS "{schemaName}";
            CREATE TABLE IF NOT EXISTS "{schemaName}".content_types (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                name VARCHAR(255) NOT NULL,
                slug VARCHAR(100) NOT NULL UNIQUE,
                fields JSONB NOT NULL DEFAULT '[]',
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS "{schemaName}".entries (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                content_type_id UUID NOT NULL REFERENCES "{schemaName}".content_types(id) ON DELETE CASCADE,
                data JSONB NOT NULL DEFAULT '{{}}',
                status VARCHAR(20) NOT NULL DEFAULT 'draft',
                published_at TIMESTAMPTZ,
                created_by UUID,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
                updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS "{schemaName}".media (
                id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                filename VARCHAR(500) NOT NULL,
                url VARCHAR(1000) NOT NULL,
                mime_type VARCHAR(100),
                size BIGINT,
                alt VARCHAR(500),
                created_by UUID,
                created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
            );
            CREATE TABLE IF NOT EXISTS "{schemaName}".settings (
                key VARCHAR(255) PRIMARY KEY,
                value JSONB NOT NULL
            );
            """);
    }

    public static string SlugToSchema(string slug) =>
        slug.Replace("-", "_").ToLowerInvariant();
}
```

- [ ] **Stap 3: Maak `TenantRepository.cs`**

```csharp
// src/Centaur.Infrastructure/Repositories/TenantRepository.cs
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class TenantRepository(CentaurDbContext context, TenantSchemaHelper schemaHelper) : ITenantRepository
{
    public Task<Tenant?> GetByIdAsync(Guid id) =>
        context.Tenants.FirstOrDefaultAsync(t => t.Id == id);

    public Task<Tenant?> GetBySlugAsync(string slug) =>
        context.Tenants.FirstOrDefaultAsync(t => t.Slug == slug);

    public async Task<IEnumerable<Tenant>> GetAllAsync() =>
        await context.Tenants.OrderBy(t => t.Name).ToListAsync();

    public async Task<Tenant> CreateAsync(Tenant tenant)
    {
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();
        await schemaHelper.CreateTenantSchemaAsync(tenant.Slug);
        return tenant;
    }

    public async Task DeleteAsync(Guid id)
    {
        var tenant = await context.Tenants.FindAsync(id);
        if (tenant is null) return;
        context.Tenants.Remove(tenant);
        await context.SaveChangesAsync();
    }

    public Task<bool> SlugExistsAsync(string slug) =>
        context.Tenants.AnyAsync(t => t.Slug == slug);
}
```

- [ ] **Stap 4: Maak `UserRepository.cs`**

```csharp
// src/Centaur.Infrastructure/Repositories/UserRepository.cs
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Centaur.Infrastructure.Repositories;

public class UserRepository(CentaurDbContext context) : IUserRepository
{
    public Task<User?> GetByEmailAsync(string email) =>
        context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public Task<User?> GetByIdAsync(Guid id) =>
        context.Users.FindAsync(id).AsTask();

    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
```

- [ ] **Stap 5: Build ter verificatie**

```bash
dotnet build src/Centaur.Infrastructure/Centaur.Infrastructure.csproj
```

Verwacht: `Build succeeded.`

- [ ] **Stap 6: Commit**

```bash
git add src/Centaur.Infrastructure/
git commit -m "feat: voeg DbContext, TenantSchemaHelper en repositories toe"
```

---

## Task 7: Database Migratie & Configuratie

**Files:**
- Wijzig: `src/Centaur.Api/appsettings.json`
- Wijzig: `src/Centaur.Api/appsettings.Development.json`
- Wijzig: `src/Centaur.Api/Program.cs`

- [ ] **Stap 1: Installeer EF Core tools en voeg EF Core toe aan Api-project**

```bash
dotnet tool install --global dotnet-ef
dotnet add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj package Microsoft.EntityFrameworkCore.Design
dotnet add src/Centaur.Api/Centaur.Api.csproj package Microsoft.EntityFrameworkCore
```

- [ ] **Stap 2: Stel `appsettings.Development.json` in**

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=centaur;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "dev-secret-key-vervang-dit-in-productie-32chars!!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- [ ] **Stap 3: Stel `appsettings.json` in**

```json
{
  "ConnectionStrings": {
    "Default": ""
  },
  "Jwt": {
    "Secret": ""
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

- [ ] **Stap 4: Stel `Program.cs` in**

```csharp
// src/Centaur.Api/Program.cs
using System.Text;
using Centaur.Application.Interfaces;
using Centaur.Application.Services;
using Centaur.Infrastructure.Data;
using Centaur.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Default")!;
var jwtSecret = builder.Configuration["Jwt:Secret"]!;

builder.Services.AddDbContext<CentaurDbContext>(opts =>
    opts.UseNpgsql(connectionString));

builder.Services.AddScoped<TenantSchemaHelper>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<AuthService>(_ => new AuthService(
    _.GetRequiredService<IUserRepository>(), jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
```

- [ ] **Stap 5: Start een lokale PostgreSQL (als die nog niet draait)**

```bash
docker run -d \
  --name centaur-db \
  -e POSTGRES_PASSWORD=postgres \
  -e POSTGRES_DB=centaur \
  -p 5432:5432 \
  postgres:16
```

- [ ] **Stap 6: Maak en voer de eerste migratie uit**

```bash
dotnet ef migrations add InitialPublicSchema \
  --project src/Centaur.Infrastructure \
  --startup-project src/Centaur.Api

dotnet ef database update \
  --project src/Centaur.Infrastructure \
  --startup-project src/Centaur.Api
```

Verwacht: `Done.`

- [ ] **Stap 7: Commit**

```bash
git add src/Centaur.Api/ src/Centaur.Infrastructure/Migrations/
git commit -m "feat: voeg EF migratie en API-configuratie toe"
```

---

## Task 8: Controllers (Auth & Tenants)

**Files:**
- Maak: `src/Centaur.Api/Controllers/AuthController.cs`
- Maak: `src/Centaur.Api/Controllers/TenantsController.cs`

- [ ] **Stap 1: Maak `AuthController.cs`**

```csharp
// src/Centaur.Api/Controllers/AuthController.cs
using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null) return Unauthorized(new { message = "Ongeldig e-mailadres of wachtwoord." });
        return Ok(result);
    }
}
```

- [ ] **Stap 2: Maak `TenantsController.cs`**

```csharp
// src/Centaur.Api/Controllers/TenantsController.cs
using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController(TenantService tenantService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (!IsSuperAdmin()) return Forbid();
        var tenants = await tenantService.GetAllAsync();
        return Ok(tenants);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTenantRequest request)
    {
        if (!IsSuperAdmin()) return Forbid();
        try
        {
            var tenant = await tenantService.CreateAsync(request);
            return CreatedAtAction(nameof(GetAll), tenant);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        if (!IsSuperAdmin()) return Forbid();
        await tenantService.DeleteAsync(id);
        return NoContent();
    }

    private bool IsSuperAdmin() =>
        User.Claims.FirstOrDefault(c => c.Type == "role")?.Value == UserRole.SuperAdmin.ToString();
}
```

- [ ] **Stap 3: Build ter verificatie**

```bash
dotnet build
```

Verwacht: `Build succeeded.`

- [ ] **Stap 4: Start de API en test handmatig**

```bash
dotnet run --project src/Centaur.Api
```

Open in de browser: `http://localhost:5000/swagger`

Verifieer dat de endpoints `POST /api/auth/login` en `GET /api/tenants` zichtbaar zijn.

- [ ] **Stap 5: Commit**

```bash
git add src/Centaur.Api/Controllers/
git commit -m "feat: voeg AuthController en TenantsController toe"
```

---

## Task 9: SuperAdmin seed bij eerste opstart

**Files:**
- Wijzig: `src/Centaur.Api/Program.cs`

- [ ] **Stap 1: Voeg seed-logica toe aan `Program.cs`** (direct na `app` aanmaken, vóór `app.Run()`)

```csharp
// Voeg toe na: var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CentaurDbContext>();
    await db.Database.MigrateAsync();

    var superAdminEmail = builder.Configuration["SuperAdmin:Email"] ?? "superadmin@centaur.nl";
    var superAdminPassword = builder.Configuration["SuperAdmin:Password"] ?? "Centaur2026!";

    if (!await db.Users.AnyAsync(u => u.Role == Centaur.Domain.Enums.UserRole.SuperAdmin))
    {
        db.Users.Add(new Centaur.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = superAdminEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(superAdminPassword),
            Role = Centaur.Domain.Enums.UserRole.SuperAdmin,
            TenantId = null,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}
```

- [ ] **Stap 2: Voeg SuperAdmin config toe aan `appsettings.Development.json`**

```json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=centaur;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "dev-secret-key-vervang-dit-in-productie-32chars!!"
  },
  "SuperAdmin": {
    "Email": "superadmin@centaur.nl",
    "Password": "Centaur2026!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

- [ ] **Stap 3: Start de API en log in als SuperAdmin**

```bash
dotnet run --project src/Centaur.Api
```

Stuur een POST naar `http://localhost:5000/api/auth/login`:

```json
{
  "email": "superadmin@centaur.nl",
  "password": "Centaur2026!"
}
```

Verwacht: een JSON-response met `accessToken`, `refreshToken`, `role: "SuperAdmin"`.

- [ ] **Stap 4: Commit**

```bash
git add src/Centaur.Api/Program.cs src/Centaur.Api/appsettings.Development.json
git commit -m "feat: voeg SuperAdmin-seed toe bij eerste opstart"
```

---

## Task 10: TenantSchema Middleware

**Files:**
- Maak: `src/Centaur.Api/Middleware/TenantSchemaMiddleware.cs`
- Wijzig: `src/Centaur.Api/Program.cs`

Deze middleware leest de `tenant_id` uit het JWT-token en stelt het juiste PostgreSQL-schema in voor de rest van de request. Plan 2 (Content API) heeft dit nodig voor elke content-aanroep.

- [ ] **Stap 1: Maak `TenantSchemaMiddleware.cs`**

```csharp
// src/Centaur.Api/Middleware/TenantSchemaMiddleware.cs
using Centaur.Infrastructure.Data;

namespace Centaur.Api.Middleware;

public class TenantSchemaMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, CentaurDbContext db)
    {
        var tenantIdClaim = context.User.Claims.FirstOrDefault(c => c.Type == "tenant_id");

        if (tenantIdClaim is not null && !string.IsNullOrEmpty(tenantIdClaim.Value)
            && Guid.TryParse(tenantIdClaim.Value, out _))
        {
            var tenantSlug = context.RequestServices
                .GetRequiredService<Centaur.Application.Interfaces.ITenantRepository>()
                .GetByIdAsync(Guid.Parse(tenantIdClaim.Value))
                .Result?.Slug;

            if (tenantSlug is not null)
            {
                var schema = TenantSchemaHelper.SlugToSchema(tenantSlug);
                context.Items["TenantSchema"] = schema;
                await db.Database.ExecuteSqlRawAsync($"SET search_path TO \"{schema}\", public");
            }
        }

        await next(context);
    }
}
```

- [ ] **Stap 2: Registreer de middleware in `Program.cs`** (na `app.UseAuthorization()`, vóór `app.MapControllers()`)

```csharp
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Centaur.Api.Middleware.TenantSchemaMiddleware>(); // voeg deze regel toe
app.MapControllers();
```

- [ ] **Stap 3: Build ter verificatie**

```bash
dotnet build
```

Verwacht: `Build succeeded.`

- [ ] **Stap 4: Commit**

```bash
git add src/Centaur.Api/Middleware/ src/Centaur.Api/Program.cs
git commit -m "feat: voeg TenantSchemaMiddleware toe voor schema-isolatie per request"
```

---

## Task 11: Eindverificatie

- [ ] **Stap 1: Voer alle tests uit**

```bash
dotnet test
```

Verwacht: alle tests slagen.

- [ ] **Stap 2: Verifieer de volledige flow**

1. Start de API: `dotnet run --project src/Centaur.Api`
2. Log in als SuperAdmin via Swagger of curl
3. Gebruik het JWT-token om een tenant aan te maken:
   ```json
   POST /api/tenants
   {
     "name": "Bakkerij De Molen",
     "slug": "de-molen",
     "adminEmail": "admin@demolen.nl",
     "adminPassword": "Wachtwoord1!"
   }
   ```
4. Verifieer in PostgreSQL dat het schema `de_molen` is aangemaakt:
   ```sql
   SELECT schema_name FROM information_schema.schemata WHERE schema_name = 'de_molen';
   ```
5. Log in als de nieuwe Admin via `/api/auth/login`

- [ ] **Stap 3: Definitieve commit**

```bash
git add .
git commit -m "feat: plan 1 voltooid — foundation en authenticatie klaar voor plan 2"
```
