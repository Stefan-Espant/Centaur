# Betaalsysteem Implementatieplan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Tenants kunnen zich gratis registreren en upgraden naar een Pro-abonnement via Stripe Checkout, met automatische limieten op API-sleutels en gebruikers voor het gratis plan.

**Architecture:** Stripe-native aanpak — `subscription_status`, `stripe_customer_id` en `stripe_subscription_id` worden opgeslagen op de `Tenant` entiteit en gesynchroniseerd via Stripe webhooks. Limieten (1 API-sleutel, 1 gebruiker op free) worden afgedwongen in de Application-laag via een statische `PlanLimits` klasse.

**Tech Stack:** Stripe.net SDK, Stripe Checkout, Stripe Customer Portal, xUnit, Moq, Nuxt 3

---

## Bestandsstructuur

**Nieuwe bestanden:**
- `src/Centaur.Application/Services/PlanLimits.cs` — statische limietregels
- `src/Centaur.Application/Interfaces/IBillingService.cs` — billing service interface
- `src/Centaur.Application/DTOs/RegisterRequest.cs` — publiek registratie DTO
- `src/Centaur.Application/DTOs/BillingStatusDto.cs` — response DTO voor /billing/status
- `src/Centaur.Infrastructure/Services/StripeBillingService.cs` — Stripe implementatie
- `src/Centaur.Api/Controllers/BillingController.cs` — billing endpoints
- `src/Centaur.Infrastructure/StripeOptions.cs` — strongly-typed configuratie POCO
- `frontend/composables/useBilling.ts` — API wrapper voor billing
- `frontend/pages/register.vue` — publieke aanmeldpagina
- `frontend/pages/billing.vue` — abonnementsbeheer in admin
- `frontend/pages/billing/success.vue` — bevestigingspagina na Checkout
- `tests/Centaur.Application.Tests/Services/PlanLimitsTests.cs`

**Gewijzigde bestanden:**
- `src/Centaur.Domain/Entities/Tenant.cs` — +3 properties
- `src/Centaur.Infrastructure/Data/CentaurDbContext.cs` — +kolomconfiguratie
- `src/Centaur.Application/Interfaces/ITenantRepository.cs` — +UpdateSubscriptionAsync, +GetByStripeCustomerIdAsync
- `src/Centaur.Infrastructure/Repositories/TenantRepository.cs` — +implementaties
- `src/Centaur.Application/Interfaces/IAuthService.cs` — +RegisterAsync
- `src/Centaur.Application/Services/AuthService.cs` — +RegisterAsync, +ITenantRepository dependency
- `src/Centaur.Application/Services/ApiKeyService.cs` — +ITenantRepository dependency, +limietcheck
- `src/Centaur.Application/Services/UserService.cs` — +ITenantRepository dependency, +limietcheck
- `src/Centaur.Api/Controllers/AuthController.cs` — +register endpoint
- `src/Centaur.Api/Program.cs` — +Stripe registratie, AuthService registratie bijwerken
- `tests/Centaur.Application.Tests/Helpers/MockTenantRepository.cs` — +nieuwe methodes
- `tests/Centaur.Application.Tests/Services/ApiKeyServiceTests.cs` — +limiettest
- `tests/Centaur.Application.Tests/Services/UserServiceTests.cs` — +limiettest

---

### Task 1: Tenant entiteit uitbreiden

**Files:**
- Modify: `src/Centaur.Domain/Entities/Tenant.cs`
- Modify: `src/Centaur.Infrastructure/Data/CentaurDbContext.cs`

- [ ] **Stap 1: Voeg 3 properties toe aan Tenant.cs**

```csharp
// src/Centaur.Domain/Entities/Tenant.cs
namespace Centaur.Domain.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public string SubscriptionStatus { get; set; } = "free";
    public string? StripeCustomerId { get; set; }
    public string? StripeSubscriptionId { get; set; }

    public ICollection<User> Users { get; set; } = new List<User>();
    public ICollection<ApiKey> ApiKeys { get; set; } = new List<ApiKey>();
}
```

- [ ] **Stap 2: Configureer kolommen in CentaurDbContext.cs**

Voeg toe aan het `modelBuilder.Entity<Tenant>` blok (na de bestaande `e.HasIndex(t => t.Slug).IsUnique()` regel):

```csharp
e.Property(t => t.SubscriptionStatus).HasMaxLength(50).HasDefaultValue("free").IsRequired();
e.Property(t => t.StripeCustomerId).HasMaxLength(255);
e.Property(t => t.StripeSubscriptionId).HasMaxLength(255);
```

- [ ] **Stap 3: Maak EF Core migratie aan**

```bash
dotnet ef migrations add AddTenantSubscriptionFields \
  --project src/Centaur.Infrastructure \
  --startup-project src/Centaur.Api
```

Verwacht: nieuw migratiebestand in `src/Centaur.Infrastructure/Migrations/`.

- [ ] **Stap 4: Commit**

```bash
git add src/Centaur.Domain/Entities/Tenant.cs \
        src/Centaur.Infrastructure/Data/CentaurDbContext.cs \
        src/Centaur.Infrastructure/Migrations/
git commit -m "feat: voeg subscription velden toe aan Tenant entiteit"
```

---

### Task 2: ITenantRepository uitbreiden

**Files:**
- Modify: `src/Centaur.Application/Interfaces/ITenantRepository.cs`
- Modify: `src/Centaur.Infrastructure/Repositories/TenantRepository.cs`
- Modify: `tests/Centaur.Application.Tests/Helpers/MockTenantRepository.cs`

- [ ] **Stap 1: Voeg twee nieuwe methodes toe aan ITenantRepository.cs**

```csharp
// src/Centaur.Application/Interfaces/ITenantRepository.cs
using Centaur.Domain.Entities;

namespace Centaur.Application.Interfaces;

public interface ITenantRepository
{
    Task<Tenant?> GetByIdAsync(Guid id);
    Task<Tenant?> GetBySlugAsync(string slug);
    Task<Tenant?> GetByStripeCustomerIdAsync(string stripeCustomerId);
    Task<IEnumerable<Tenant>> GetAllAsync();
    Task<Tenant> CreateAsync(Tenant tenant);
    Task DeleteAsync(Guid id);
    Task<bool> SlugExistsAsync(string slug);
    Task UpdateSubscriptionAsync(Guid tenantId, string? stripeCustomerId, string? stripeSubscriptionId, string subscriptionStatus);
}
```

- [ ] **Stap 2: Implementeer in TenantRepository.cs**

Voeg toe onderaan de class (voor de sluitende `}`):

```csharp
public Task<Tenant?> GetByStripeCustomerIdAsync(string stripeCustomerId) =>
    context.Tenants.FirstOrDefaultAsync(t => t.StripeCustomerId == stripeCustomerId);

public async Task UpdateSubscriptionAsync(Guid tenantId, string? stripeCustomerId, string? stripeSubscriptionId, string subscriptionStatus)
{
    var tenant = await context.Tenants.FindAsync(tenantId);
    if (tenant is null) return;
    tenant.StripeCustomerId = stripeCustomerId;
    tenant.StripeSubscriptionId = stripeSubscriptionId;
    tenant.SubscriptionStatus = subscriptionStatus;
    await context.SaveChangesAsync();
}
```

- [ ] **Stap 3: Implementeer in MockTenantRepository.cs**

Voeg toe onderaan de class:

```csharp
public Task<Tenant?> GetByStripeCustomerIdAsync(string stripeCustomerId) =>
    Task.FromResult(_tenants.FirstOrDefault(t => t.StripeCustomerId == stripeCustomerId));

public Task UpdateSubscriptionAsync(Guid tenantId, string? stripeCustomerId, string? stripeSubscriptionId, string subscriptionStatus)
{
    var tenant = _tenants.FirstOrDefault(t => t.Id == tenantId);
    if (tenant is null) return Task.CompletedTask;
    tenant.StripeCustomerId = stripeCustomerId;
    tenant.StripeSubscriptionId = stripeSubscriptionId;
    tenant.SubscriptionStatus = subscriptionStatus;
    return Task.CompletedTask;
}
```

- [ ] **Stap 4: Voer tests uit — verwacht geen fouten**

```bash
dotnet test tests/Centaur.Application.Tests/ --no-build 2>/dev/null || dotnet test tests/Centaur.Application.Tests/
```

Verwacht: alle bestaande tests slagen.

- [ ] **Stap 5: Commit**

```bash
git add src/Centaur.Application/Interfaces/ITenantRepository.cs \
        src/Centaur.Infrastructure/Repositories/TenantRepository.cs \
        tests/Centaur.Application.Tests/Helpers/MockTenantRepository.cs
git commit -m "feat: voeg UpdateSubscriptionAsync en GetByStripeCustomerIdAsync toe aan TenantRepository"
```

---

### Task 3: PlanLimits klasse + tests

**Files:**
- Create: `src/Centaur.Application/Services/PlanLimits.cs`
- Create: `tests/Centaur.Application.Tests/Services/PlanLimitsTests.cs`

- [ ] **Stap 1: Schrijf de falende tests**

```csharp
// tests/Centaur.Application.Tests/Services/PlanLimitsTests.cs
using Centaur.Application.Services;

namespace Centaur.Application.Tests.Services;

public class PlanLimitsTests
{
    [Theory]
    [InlineData("free", 1)]
    [InlineData("canceled", 1)]
    [InlineData("past_due", int.MaxValue)]
    [InlineData("active", int.MaxValue)]
    public void MaxApiKeys_ReturnsCorrectLimit(string status, int expected)
    {
        Assert.Equal(expected, PlanLimits.MaxApiKeys(status));
    }

    [Theory]
    [InlineData("free", 1)]
    [InlineData("canceled", 1)]
    [InlineData("past_due", int.MaxValue)]
    [InlineData("active", int.MaxValue)]
    public void MaxUsers_ReturnsCorrectLimit(string status, int expected)
    {
        Assert.Equal(expected, PlanLimits.MaxUsers(status));
    }
}
```

- [ ] **Stap 2: Verifieer dat tests falen**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "PlanLimitsTests"
```

Verwacht: FAIL met "The type or namespace name 'PlanLimits' could not be found".

- [ ] **Stap 3: Maak PlanLimits.cs aan**

```csharp
// src/Centaur.Application/Services/PlanLimits.cs
namespace Centaur.Application.Services;

public static class PlanLimits
{
    public static int MaxApiKeys(string subscriptionStatus) =>
        subscriptionStatus is "active" or "past_due" ? int.MaxValue : 1;

    public static int MaxUsers(string subscriptionStatus) =>
        subscriptionStatus is "active" or "past_due" ? int.MaxValue : 1;
}
```

- [ ] **Stap 4: Verifieer dat tests slagen**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "PlanLimitsTests"
```

Verwacht: PASS (4 tests).

- [ ] **Stap 5: Commit**

```bash
git add src/Centaur.Application/Services/PlanLimits.cs \
        tests/Centaur.Application.Tests/Services/PlanLimitsTests.cs
git commit -m "feat: voeg PlanLimits toe voor free/active plan enforcement"
```

---

### Task 4: ApiKeyService — limiet enforcement

**Files:**
- Modify: `src/Centaur.Application/Services/ApiKeyService.cs`
- Modify: `tests/Centaur.Application.Tests/Services/ApiKeyServiceTests.cs`

- [ ] **Stap 1: Schrijf de falende test**

Open `tests/Centaur.Application.Tests/Services/ApiKeyServiceTests.cs`. Voeg toe (na de bestaande tests):

```csharp
[Fact]
public async Task CreateAsync_WhenFreePlanAndLimitReached_Throws()
{
    var tenantId = Guid.NewGuid();
    var tenant = new Tenant { Id = tenantId, Slug = "t", Name = "T", SubscriptionStatus = "free", CreatedAt = DateTime.UtcNow };
    _tenantRepo.Seed(tenant);

    // Seed een bestaande API-sleutel — limiet = 1
    _apiKeyRepo.Seed(new ApiKey { Id = Guid.NewGuid(), Label = "Bestaand", KeyHash = "x", TenantId = tenantId, CreatedAt = DateTime.UtcNow });

    var service = new ApiKeyService(_apiKeyRepo, _tenantRepo);
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.CreateAsync(tenantId, new CreateApiKeyRequest("Nieuw", null)));
}
```

Zorg dat boven in het testbestand de `_tenantRepo` field is gedeclareerd:

```csharp
private readonly MockTenantRepository _tenantRepo = new();
```

En dat `_apiKeyRepo` van type `MockApiKeyRepository` is (kijk of dat al bestaat, anders: `private readonly MockApiKeyRepository _apiKeyRepo = new();`).

- [ ] **Stap 2: Verifieer dat de test faalt**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "ApiKeyServiceTests"
```

Verwacht: compileerfout of FAIL doordat de limietcheck nog niet bestaat.

- [ ] **Stap 3: Voeg limietcheck toe aan ApiKeyService.cs**

Vervang het volledige bestand:

```csharp
// src/Centaur.Application/Services/ApiKeyService.cs
using System.Security.Cryptography;
using System.Text;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;

namespace Centaur.Application.Services;

public class ApiKeyService(IApiKeyRepository repository, ITenantRepository tenantRepository) : IApiKeyService
{
    public async Task<IEnumerable<ApiKeyDto>> GetByTenantIdAsync(Guid tenantId)
    {
        var keys = await repository.GetByTenantIdAsync(tenantId);
        return keys.Select(k => new ApiKeyDto(k.Id, k.Label, k.TenantId, k.ExpiresAt, k.CreatedAt));
    }

    public async Task<CreatedApiKeyDto> CreateAsync(Guid tenantId, CreateApiKeyRequest request)
    {
        var tenant = await tenantRepository.GetByIdAsync(tenantId);
        var status = tenant?.SubscriptionStatus ?? "free";
        var existing = await repository.GetByTenantIdAsync(tenantId);
        if (existing.Count() >= PlanLimits.MaxApiKeys(status))
            throw new InvalidOperationException("API-sleutellimiet bereikt voor dit plan.");

        var rawKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var keyHash = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(rawKey)));

        var apiKey = await repository.CreateAsync(new ApiKey
        {
            Id = Guid.NewGuid(),
            Label = request.Label,
            KeyHash = keyHash,
            TenantId = tenantId,
            ExpiresAt = request.ExpiresAt,
            CreatedAt = DateTime.UtcNow
        });

        return new CreatedApiKeyDto(apiKey.Id, apiKey.Label, rawKey, apiKey.TenantId, apiKey.ExpiresAt, apiKey.CreatedAt);
    }

    public Task DeleteAsync(Guid id) => repository.DeleteAsync(id);
}
```

- [ ] **Stap 4: Verifieer dat alle ApiKeyServiceTests slagen**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "ApiKeyServiceTests"
```

Verwacht: PASS.

- [ ] **Stap 5: Controleer of bestaande tests die ApiKeyService aanmaken nog compileren**

Zoek in de tests naar `new ApiKeyService(` en pas de constructor aan als die nu ook `ITenantRepository` nodig heeft. Pas aan waar nodig: `new ApiKeyService(_apiKeyRepo, _tenantRepo)`.

- [ ] **Stap 6: Commit**

```bash
git add src/Centaur.Application/Services/ApiKeyService.cs \
        tests/Centaur.Application.Tests/Services/ApiKeyServiceTests.cs
git commit -m "feat: dwing API-sleutellimiet af op free plan"
```

---

### Task 5: UserService — limiet enforcement

**Files:**
- Modify: `src/Centaur.Application/Services/UserService.cs`
- Modify: `tests/Centaur.Application.Tests/Services/UserServiceTests.cs`

- [ ] **Stap 1: Schrijf de falende test**

Open `tests/Centaur.Application.Tests/Services/UserServiceTests.cs`. Voeg toe:

```csharp
[Fact]
public async Task CreateAsync_WhenFreePlanAndLimitReached_Throws()
{
    var tenantId = Guid.NewGuid();
    var tenant = new Tenant { Id = tenantId, Slug = "t", Name = "T", SubscriptionStatus = "free", CreatedAt = DateTime.UtcNow };
    _tenantRepo.Seed(tenant);

    // Seed één bestaande gebruiker — limiet = 1
    _userRepo.Seed(new User { Id = Guid.NewGuid(), Email = "bestaand@test.nl", PasswordHash = "x", Role = UserRole.Admin, TenantId = tenantId, CreatedAt = DateTime.UtcNow });

    var service = new UserService(_userRepo, _tenantRepo);
    await Assert.ThrowsAsync<InvalidOperationException>(
        () => service.CreateAsync(tenantId, new CreateUserRequest("nieuw@test.nl", "Wachtwoord1!", "Admin")));
}
```

Voeg bovenaan toe als het nog niet bestaat:

```csharp
private readonly MockTenantRepository _tenantRepo = new();
private readonly MockUserRepository _userRepo = new();
```

- [ ] **Stap 2: Verifieer dat de test faalt**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "UserServiceTests"
```

Verwacht: FAIL.

- [ ] **Stap 3: Voeg limietcheck toe aan UserService.cs**

Vervang het volledige bestand:

```csharp
// src/Centaur.Application/Services/UserService.cs
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;

namespace Centaur.Application.Services;

public class UserService(IUserRepository userRepository, ITenantRepository tenantRepository) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetByTenantIdAsync(Guid tenantId)
    {
        var users = await userRepository.GetByTenantIdAsync(tenantId);
        return users.Select(u => new UserDto(u.Id, u.Email, u.Role.ToString(), u.TenantId, u.CreatedAt));
    }

    public async Task<UserDto> CreateAsync(Guid tenantId, CreateUserRequest request)
    {
        if (!Enum.TryParse<UserRole>(request.Role, out var role) || role == UserRole.SuperAdmin)
            throw new ArgumentException($"Ongeldige rol: {request.Role}");

        var tenant = await tenantRepository.GetByIdAsync(tenantId);
        var status = tenant?.SubscriptionStatus ?? "free";
        var existing = await userRepository.GetByTenantIdAsync(tenantId);
        if (existing.Count() >= PlanLimits.MaxUsers(status))
            throw new InvalidOperationException("Gebruikerslimiet bereikt voor dit plan.");

        var user = await userRepository.CreateAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(request.Password)),
            Role = role,
            TenantId = tenantId,
            CreatedAt = DateTime.UtcNow
        });

        return new UserDto(user.Id, user.Email, user.Role.ToString(), user.TenantId, user.CreatedAt);
    }

    public Task DeleteAsync(Guid id) => userRepository.DeleteAsync(id);
}
```

- [ ] **Stap 4: Verifieer dat alle UserServiceTests slagen**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "UserServiceTests"
```

Verwacht: PASS.

- [ ] **Stap 5: Update DI in Program.cs — UserService heeft nu ook ITenantRepository nodig**

Geen actie vereist: Program.cs gebruikt `AddScoped<IUserService, UserService>()` met constructor injection. De DI container injecteert `ITenantRepository` automatisch omdat `ITenantRepository` al geregistreerd is.

Controleer wel of alle tests nog slagen:

```bash
dotnet test tests/Centaur.Application.Tests/
```

Verwacht: alle tests slagen.

- [ ] **Stap 6: Commit**

```bash
git add src/Centaur.Application/Services/UserService.cs \
        tests/Centaur.Application.Tests/Services/UserServiceTests.cs
git commit -m "feat: dwing gebruikerslimiet af op free plan"
```

---

### Task 6: Publieke register endpoint

**Files:**
- Create: `src/Centaur.Application/DTOs/RegisterRequest.cs`
- Modify: `src/Centaur.Application/Interfaces/IAuthService.cs`
- Modify: `src/Centaur.Application/Services/AuthService.cs`
- Modify: `src/Centaur.Api/Controllers/AuthController.cs`
- Modify: `src/Centaur.Api/Program.cs`

- [ ] **Stap 1: Schrijf de falende test**

Open `tests/Centaur.Application.Tests/Services/AuthServiceTests.cs`. Voeg toe:

```csharp
[Fact]
public async Task RegisterAsync_CreatesNewTenantAndReturnsJwt()
{
    var userRepo = new MockUserRepository();
    var tenantRepo = new MockTenantRepository();
    var service = new AuthService(userRepo, tenantRepo, "test-secret-minimaal-32-tekens-lang!!");

    var result = await service.RegisterAsync(new RegisterRequest("Bakkerij De Molen", "admin@demolen.nl", "Wachtwoord1!"));

    Assert.NotNull(result.AccessToken);
    Assert.Equal("Admin", result.Role);
    Assert.NotNull(result.TenantId);
    Assert.Single(tenantRepo.GetAll());
}
```

Voeg bovenaan toe als nog niet aanwezig:

```csharp
using Centaur.Application.DTOs;
using Centaur.Application.Services;
using Centaur.Application.Tests.Helpers;
```

En voeg een publieke `GetAll()` toe aan `MockTenantRepository` zodat de test kan werken:

In `MockTenantRepository.cs`:
```csharp
public IReadOnlyList<Tenant> GetAll() => _tenants.AsReadOnly();
```

- [ ] **Stap 2: Verifieer dat de test faalt**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "RegisterAsync"
```

Verwacht: compileerfout.

- [ ] **Stap 3: Maak RegisterRequest.cs aan**

```csharp
// src/Centaur.Application/DTOs/RegisterRequest.cs
namespace Centaur.Application.DTOs;

public record RegisterRequest(string TenantName, string Email, string Password);
```

- [ ] **Stap 4: Breid IAuthService.cs uit**

```csharp
// src/Centaur.Application/Interfaces/IAuthService.cs
using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
}
```

- [ ] **Stap 5: Implementeer RegisterAsync in AuthService.cs**

Vervang het volledige bestand:

```csharp
// src/Centaur.Application/Services/AuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Centaur.Domain.Entities;
using Centaur.Domain.Enums;
using Microsoft.IdentityModel.Tokens;

namespace Centaur.Application.Services;

public class AuthService(IUserRepository userRepository, ITenantRepository tenantRepository, string jwtSecret) : IAuthService
{
    private readonly string _jwtSecret = Encoding.UTF8.GetByteCount(jwtSecret) >= 32
        ? jwtSecret
        : throw new ArgumentException("JWT secret must be at least 32 bytes.", nameof(jwtSecret));

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        var accessToken = GenerateToken(user, TimeSpan.FromMinutes(15), "access");
        var refreshToken = GenerateToken(user, TimeSpan.FromDays(7), "refresh");

        return new AuthResponse(accessToken, refreshToken, user.Role.ToString(), user.TenantId);
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var slug = Regex.Replace(request.TenantName.ToLowerInvariant(), @"\s+", "-");
        slug = Regex.Replace(slug, @"[^a-z0-9-]", "");

        var baseSlug = slug;
        var counter = 1;
        while (await tenantRepository.SlugExistsAsync(slug))
            slug = $"{baseSlug}-{counter++}";

        var tenant = await tenantRepository.CreateAsync(new Tenant
        {
            Id = Guid.NewGuid(),
            Slug = slug,
            Name = request.TenantName,
            SubscriptionStatus = "free",
            CreatedAt = DateTime.UtcNow
        });

        var user = await userRepository.CreateAsync(new User
        {
            Id = Guid.NewGuid(),
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(request.Password)),
            Role = UserRole.Admin,
            TenantId = tenant.Id,
            CreatedAt = DateTime.UtcNow
        });

        var accessToken = GenerateToken(user, TimeSpan.FromMinutes(15), "access");
        var refreshToken = GenerateToken(user, TimeSpan.FromDays(7), "refresh");

        return new AuthResponse(accessToken, refreshToken, user.Role.ToString(), user.TenantId);
    }

    private string GenerateToken(User user, TimeSpan expiry, string tokenType)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var claims = new[]
        {
            new Claim("user_id", user.Id.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim("tenant_id", user.TenantId?.ToString() ?? string.Empty),
            new Claim("token_type", tokenType)
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.Add(expiry),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
```

- [ ] **Stap 6: Verifieer dat de test slaagt**

```bash
dotnet test tests/Centaur.Application.Tests/ --filter "RegisterAsync"
```

Verwacht: PASS.

- [ ] **Stap 7: Voeg register endpoint toe aan AuthController.cs**

```csharp
// src/Centaur.Api/Controllers/AuthController.cs
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        if (result is null) return Unauthorized(new { message = "Ongeldig e-mailadres of wachtwoord." });
        return Ok(result);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await authService.RegisterAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
```

- [ ] **Stap 8: Update AuthService registratie in Program.cs**

Vervang de bestaande `AuthService` registratie:

```csharp
// Oud:
builder.Services.AddScoped<IAuthService>(_ => new AuthService(
    _.GetRequiredService<IUserRepository>(), jwtSecret));

// Nieuw:
builder.Services.AddScoped<IAuthService>(_ => new AuthService(
    _.GetRequiredService<IUserRepository>(),
    _.GetRequiredService<ITenantRepository>(),
    jwtSecret));
```

- [ ] **Stap 9: Bouw de applicatie**

```bash
dotnet build src/Centaur.Api
```

Verwacht: Build succeeded.

- [ ] **Stap 10: Commit**

```bash
git add src/Centaur.Application/DTOs/RegisterRequest.cs \
        src/Centaur.Application/Interfaces/IAuthService.cs \
        src/Centaur.Application/Services/AuthService.cs \
        src/Centaur.Api/Controllers/AuthController.cs \
        src/Centaur.Api/Program.cs \
        tests/Centaur.Application.Tests/Helpers/MockTenantRepository.cs \
        tests/Centaur.Application.Tests/Services/AuthServiceTests.cs
git commit -m "feat: voeg publieke register endpoint toe"
```

---

### Task 7: Stripe NuGet package + configuratie

**Files:**
- Modify: `src/Centaur.Infrastructure/Centaur.Infrastructure.csproj`
- Create: `src/Centaur.Api/StripeOptions.cs`
- Modify: `src/Centaur.Api/appsettings.json`
- Modify: `src/Centaur.Api/Program.cs`

- [ ] **Stap 1: Voeg Stripe.net NuGet toe aan Infrastructure project**

```bash
dotnet add src/Centaur.Infrastructure package Stripe.net
```

Verwacht: package toegevoegd aan `Centaur.Infrastructure.csproj`.

- [ ] **Stap 2: Maak StripeOptions.cs aan in Infrastructure (niet Api)**

`StripeOptions` hoort in Infrastructure omdat `StripeBillingService` het gebruikt. Infrastructure mag niet afhankelijk zijn van Api.

```csharp
// src/Centaur.Infrastructure/StripeOptions.cs
namespace Centaur.Infrastructure;

public class StripeOptions
{
    public string SecretKey { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string ProPriceId { get; set; } = string.Empty;
    public string FrontendUrl { get; set; } = "http://localhost:3000";
}
```

- [ ] **Stap 3: Voeg Stripe sectie toe aan appsettings.json**

Open `src/Centaur.Api/appsettings.json` en voeg toe aan het root object:

```json
"Stripe": {
  "SecretKey": "",
  "WebhookSecret": "",
  "ProPriceId": "",
  "FrontendUrl": "http://localhost:3000"
}
```

**Let op: commit appsettings.json nooit met echte waarden. Echte sleutels gaan in omgevingsvariabelen of user-secrets.**

- [ ] **Stap 4: Registreer StripeOptions in Program.cs**

Voeg toe na `var jwtSecret = ...`:

```csharp
builder.Services.Configure<Centaur.Infrastructure.StripeOptions>(
    builder.Configuration.GetSection("Stripe"));
```

- [ ] **Stap 5: Bouw de applicatie**

```bash
dotnet build src/Centaur.Api
```

Verwacht: Build succeeded.

- [ ] **Stap 6: Commit**

```bash
git add src/Centaur.Infrastructure/Centaur.Infrastructure.csproj \
        src/Centaur.Infrastructure/StripeOptions.cs \
        src/Centaur.Api/appsettings.json \
        src/Centaur.Api/Program.cs
git commit -m "feat: voeg Stripe.net package en configuratie toe"
```

---

### Task 8: IBillingService + StripeBillingService

**Files:**
- Create: `src/Centaur.Application/DTOs/BillingStatusDto.cs`
- Create: `src/Centaur.Application/Interfaces/IBillingService.cs`
- Create: `src/Centaur.Infrastructure/Services/StripeBillingService.cs`
- Modify: `src/Centaur.Api/Program.cs`

- [ ] **Stap 1: Maak BillingStatusDto.cs aan**

```csharp
// src/Centaur.Application/DTOs/BillingStatusDto.cs
namespace Centaur.Application.DTOs;

public record BillingStatusDto(string SubscriptionStatus, string? NextBillingDate);
```

- [ ] **Stap 2: Maak IBillingService.cs aan**

```csharp
// src/Centaur.Application/Interfaces/IBillingService.cs
using Centaur.Application.DTOs;

namespace Centaur.Application.Interfaces;

public interface IBillingService
{
    Task<string> CreateCheckoutSessionAsync(Guid tenantId, string customerEmail);
    Task<string> CreatePortalSessionAsync(Guid tenantId);
    Task<BillingStatusDto> GetStatusAsync(Guid tenantId);
    Task HandleWebhookAsync(string payload, string signature);
}
```

- [ ] **Stap 3: Maak StripeBillingService.cs aan**

```csharp
// src/Centaur.Infrastructure/Services/StripeBillingService.cs
using Centaur.Application.DTOs;
using Centaur.Application.Interfaces;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Centaur.Infrastructure.Services;

public class StripeBillingService(
    ITenantRepository tenantRepository,
    IOptions<StripeOptions> options) : IBillingService
{
    private readonly StripeOptions _options = options.Value;

    public async Task<string> CreateCheckoutSessionAsync(Guid tenantId, string customerEmail)
    {
        StripeConfiguration.ApiKey = _options.SecretKey;

        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant niet gevonden.");

        string stripeCustomerId;
        if (!string.IsNullOrEmpty(tenant.StripeCustomerId))
        {
            stripeCustomerId = tenant.StripeCustomerId;
        }
        else
        {
            var customerService = new CustomerService();
            var customer = await customerService.CreateAsync(new CustomerCreateOptions
            {
                Email = customerEmail,
                Metadata = new Dictionary<string, string> { ["tenant_id"] = tenantId.ToString() }
            });
            stripeCustomerId = customer.Id;
            await tenantRepository.UpdateSubscriptionAsync(tenantId, stripeCustomerId, tenant.StripeSubscriptionId, tenant.SubscriptionStatus);
        }

        var sessionService = new SessionService();
        var session = await sessionService.CreateAsync(new SessionCreateOptions
        {
            Customer = stripeCustomerId,
            Mode = "subscription",
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Price = _options.ProPriceId,
                    Quantity = 1
                }
            ],
            Metadata = new Dictionary<string, string> { ["tenant_id"] = tenantId.ToString() },
            SuccessUrl = $"{_options.FrontendUrl}/billing/success",
            CancelUrl = $"{_options.FrontendUrl}/billing"
        });

        return session.Url;
    }

    public async Task<string> CreatePortalSessionAsync(Guid tenantId)
    {
        StripeConfiguration.ApiKey = _options.SecretKey;

        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant niet gevonden.");

        if (string.IsNullOrEmpty(tenant.StripeCustomerId))
            throw new InvalidOperationException("Geen Stripe klant gevonden voor deze tenant.");

        var portalService = new Stripe.BillingPortal.SessionService();
        var session = await portalService.CreateAsync(new Stripe.BillingPortal.SessionCreateOptions
        {
            Customer = tenant.StripeCustomerId,
            ReturnUrl = $"{_options.FrontendUrl}/billing"
        });

        return session.Url;
    }

    public async Task<BillingStatusDto> GetStatusAsync(Guid tenantId)
    {
        StripeConfiguration.ApiKey = _options.SecretKey;

        var tenant = await tenantRepository.GetByIdAsync(tenantId)
            ?? throw new InvalidOperationException("Tenant niet gevonden.");

        if (string.IsNullOrEmpty(tenant.StripeSubscriptionId))
            return new BillingStatusDto(tenant.SubscriptionStatus, null);

        var subscriptionService = new SubscriptionService();
        var subscription = await subscriptionService.GetAsync(tenant.StripeSubscriptionId);
        var nextBillingDate = subscription.CurrentPeriodEnd.ToString("yyyy-MM-dd");

        return new BillingStatusDto(tenant.SubscriptionStatus, nextBillingDate);
    }

    public async Task HandleWebhookAsync(string payload, string signature)
    {
        StripeConfiguration.ApiKey = _options.SecretKey;

        var stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret);

        switch (stripeEvent.Type)
        {
            case EventTypes.CheckoutSessionCompleted:
                var session = (Session)stripeEvent.Data.Object;
                if (session.Metadata.TryGetValue("tenant_id", out var tenantIdStr)
                    && Guid.TryParse(tenantIdStr, out var tenantId))
                {
                    await tenantRepository.UpdateSubscriptionAsync(
                        tenantId,
                        session.CustomerId,
                        session.SubscriptionId,
                        "active");
                }
                break;

            case EventTypes.CustomerSubscriptionUpdated:
                var updatedSub = (Subscription)stripeEvent.Data.Object;
                var updatedTenant = await tenantRepository.GetByStripeCustomerIdAsync(updatedSub.CustomerId);
                if (updatedTenant is not null)
                {
                    await tenantRepository.UpdateSubscriptionAsync(
                        updatedTenant.Id,
                        updatedTenant.StripeCustomerId,
                        updatedSub.Id,
                        updatedSub.Status);
                }
                break;

            case EventTypes.CustomerSubscriptionDeleted:
                var deletedSub = (Subscription)stripeEvent.Data.Object;
                var deletedTenant = await tenantRepository.GetByStripeCustomerIdAsync(deletedSub.CustomerId);
                if (deletedTenant is not null)
                {
                    await tenantRepository.UpdateSubscriptionAsync(
                        deletedTenant.Id,
                        deletedTenant.StripeCustomerId,
                        null,
                        "canceled");
                }
                break;
        }
    }
}
```

- [ ] **Stap 4: Registreer StripeBillingService in Program.cs**

Voeg toe aan de service registraties (voor `var app = builder.Build();`):

```csharp
builder.Services.AddScoped<IBillingService, Centaur.Infrastructure.Services.StripeBillingService>();
```

- [ ] **Stap 5: Bouw de applicatie**

```bash
dotnet build src/Centaur.Api
```

Verwacht: Build succeeded.

- [ ] **Stap 6: Commit**

```bash
git add src/Centaur.Application/DTOs/BillingStatusDto.cs \
        src/Centaur.Application/Interfaces/IBillingService.cs \
        src/Centaur.Infrastructure/Services/StripeBillingService.cs \
        src/Centaur.Api/Program.cs
git commit -m "feat: voeg StripeBillingService toe (checkout, portal, status, webhooks)"
```

---

### Task 9: BillingController

**Files:**
- Create: `src/Centaur.Api/Controllers/BillingController.cs`

- [ ] **Stap 1: Maak BillingController.cs aan**

```csharp
// src/Centaur.Api/Controllers/BillingController.cs
using Centaur.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Centaur.Api.Controllers;

[ApiController]
[Route("api/billing")]
public class BillingController(IBillingService billingService) : ControllerBase
{
    private Guid? CurrentTenantId => Guid.TryParse(
        User.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value, out var id)
            ? id
            : null;

    private string? CurrentEmail =>
        User.Claims.FirstOrDefault(c => c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
        ?? User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;

    [HttpGet("status")]
    [Authorize]
    public async Task<IActionResult> GetStatus()
    {
        if (CurrentTenantId is null) return Forbid();
        var status = await billingService.GetStatusAsync(CurrentTenantId.Value);
        return Ok(status);
    }

    [HttpPost("checkout")]
    [Authorize]
    public async Task<IActionResult> CreateCheckout()
    {
        if (CurrentTenantId is null) return Forbid();

        try
        {
            var email = CurrentEmail ?? string.Empty;
            var url = await billingService.CreateCheckoutSessionAsync(CurrentTenantId.Value, email);
            return Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("portal")]
    [Authorize]
    public async Task<IActionResult> CreatePortal()
    {
        if (CurrentTenantId is null) return Forbid();

        try
        {
            var url = await billingService.CreatePortalSessionAsync(CurrentTenantId.Value);
            return Ok(new { url });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook()
    {
        var payload = await new StreamReader(Request.Body).ReadToEndAsync();
        var signature = Request.Headers["Stripe-Signature"].ToString();

        try
        {
            await billingService.HandleWebhookAsync(payload, signature);
            return Ok();
        }
        catch (Stripe.StripeException)
        {
            return BadRequest();
        }
    }
}
```

- [ ] **Stap 2: Voeg e-mail claim toe aan JWT in AuthService.cs**

Het e-mailadres van de gebruiker moet als claim in de JWT staan zodat de BillingController het e-mailadres kan uitlezen voor de Stripe Customer. Voeg toe aan de `claims` array in `GenerateToken`:

```csharp
new Claim("email", user.Email),
```

De volledige `claims` array wordt:

```csharp
var claims = new[]
{
    new Claim("user_id", user.Id.ToString()),
    new Claim("role", user.Role.ToString()),
    new Claim("tenant_id", user.TenantId?.ToString() ?? string.Empty),
    new Claim("token_type", tokenType),
    new Claim("email", user.Email)
};
```

Update ook de `CurrentEmail` property in BillingController om de juiste claim te gebruiken:

```csharp
private string? CurrentEmail =>
    User.Claims.FirstOrDefault(c => c.Type == "email")?.Value;
```

- [ ] **Stap 3: Bouw de applicatie**

```bash
dotnet build src/Centaur.Api
```

Verwacht: Build succeeded.

- [ ] **Stap 4: Voer alle tests uit**

```bash
dotnet test tests/Centaur.Application.Tests/
```

Verwacht: alle tests slagen.

- [ ] **Stap 5: Commit**

```bash
git add src/Centaur.Api/Controllers/BillingController.cs \
        src/Centaur.Application/Services/AuthService.cs
git commit -m "feat: voeg BillingController toe met checkout, portal, status en webhook endpoints"
```

---

### Task 10: Frontend — register pagina

**Files:**
- Create: `frontend/pages/register.vue`
- Modify: `frontend/pages/login.vue`

- [ ] **Stap 1: Maak register.vue aan**

```vue
<!-- frontend/pages/register.vue -->
<script setup lang="ts">
definePageMeta({ layout: 'auth' })

const config = useRuntimeConfig()
const router = useRouter()
const { login } = useAuth()

const form = reactive({ tenantName: '', email: '', password: '' })
const error = ref('')
const loading = ref(false)

async function submit() {
  error.value = ''
  loading.value = true
  try {
    const data = await $fetch<{ accessToken: string }>(`${config.public.apiBase}/api/auth/register`, {
      method: 'POST',
      body: { tenantName: form.tenantName, email: form.email, password: form.password }
    })
    localStorage.setItem('centaur_token', data.accessToken)
    await router.push('/dashboard')
  } catch (e: unknown) {
    const err = e as { data?: { message?: string }; message?: string }
    error.value = err?.data?.message ?? err?.message ?? 'Registratie mislukt'
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="auth-box">
    <div class="auth-logo">
      <AppLogo height="44px" />
    </div>
    <div class="auth-title">Account aanmaken</div>
    <form @submit.prevent="submit">
      <div class="form-group">
        <label class="form-label">Bedrijfsnaam</label>
        <input v-model="form.tenantName" type="text" class="form-input" required autocomplete="organization" />
      </div>
      <div class="form-group">
        <label class="form-label">E-mailadres</label>
        <input v-model="form.email" type="email" class="form-input" required autocomplete="email" />
      </div>
      <div class="form-group">
        <label class="form-label">Wachtwoord</label>
        <input v-model="form.password" type="password" class="form-input" required autocomplete="new-password" />
      </div>
      <div v-if="error" class="form-error">{{ error }}</div>
      <button type="submit" class="btn btn-primary" style="width:100%;margin-top:1.25rem" :disabled="loading">
        {{ loading ? 'Bezig...' : 'Gratis starten' }}
      </button>
    </form>
    <div style="margin-top:1rem;text-align:center;font-size:13px">
      Al een account? <NuxtLink to="/login">Inloggen</NuxtLink>
    </div>
  </div>
</template>
```

- [ ] **Stap 2: Voeg link naar register toe aan login.vue**

Voeg toe onderaan het `<template>` in login.vue (voor de sluitende `</div>`):

```html
<div style="margin-top:1rem;text-align:center;font-size:13px">
  Nog geen account? <NuxtLink to="/register">Gratis registreren</NuxtLink>
</div>
```

- [ ] **Stap 3: Commit**

```bash
git add frontend/pages/register.vue frontend/pages/login.vue
git commit -m "feat: voeg publieke registratiepagina toe"
```

---

### Task 11: Frontend — billing pagina

**Files:**
- Create: `frontend/composables/useBilling.ts`
- Create: `frontend/pages/billing.vue`
- Create: `frontend/pages/billing/success.vue`

- [ ] **Stap 1: Maak useBilling.ts aan**

```typescript
// frontend/composables/useBilling.ts
export interface BillingStatus {
  subscriptionStatus: string
  nextBillingDate: string | null
}

export const useBilling = () => {
  const { get, post } = useApi()

  async function getStatus(): Promise<BillingStatus> {
    return get<BillingStatus>('/api/billing/status')
  }

  async function startCheckout(): Promise<void> {
    const { url } = await post<{ url: string }>('/api/billing/checkout', {})
    window.location.href = url
  }

  async function openPortal(): Promise<void> {
    const { url } = await post<{ url: string }>('/api/billing/portal', {})
    window.location.href = url
  }

  return { getStatus, startCheckout, openPortal }
}
```

- [ ] **Stap 2: Maak billing.vue aan**

```vue
<!-- frontend/pages/billing.vue -->
<script setup lang="ts">
import type { BillingStatus } from '~/composables/useBilling'

definePageMeta({ middleware: 'auth' })

const { getStatus, startCheckout, openPortal } = useBilling()

const status = ref<BillingStatus | null>(null)
const error = ref('')
const loading = ref(false)

onMounted(async () => {
  try {
    status.value = await getStatus()
  } catch (e: unknown) {
    error.value = (e as Error).message
  }
})

async function upgrade() {
  loading.value = true
  try {
    await startCheckout()
  } catch (e: unknown) {
    error.value = (e as Error).message
    loading.value = false
  }
}

async function manage() {
  loading.value = true
  try {
    await openPortal()
  } catch (e: unknown) {
    error.value = (e as Error).message
    loading.value = false
  }
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('nl-NL', { day: 'numeric', month: 'long', year: 'numeric' })
}

const isPro = computed(() => status.value?.subscriptionStatus === 'active' || status.value?.subscriptionStatus === 'past_due')
const isCanceled = computed(() => status.value?.subscriptionStatus === 'canceled')
const isPastDue = computed(() => status.value?.subscriptionStatus === 'past_due')
</script>

<template>
  <div style="max-width:480px">
    <div class="content-header">
      <div class="content-title">Abonnement</div>
    </div>

    <div v-if="error" class="alert alert-error">{{ error }}</div>

    <div v-if="status" class="panel stack">
      <!-- Past due banner -->
      <div v-if="isPastDue" class="alert alert-error" style="margin-bottom:0">
        ⚠ Betaling mislukt — Stripe herprobeert automatisch.
      </div>

      <!-- Pro actief -->
      <template v-if="isPro">
        <div>
          <div style="font-weight:600;font-size:1.05rem">Pro <span style="color:#16a34a">● Actief</span></div>
          <div v-if="status.nextBillingDate && !isPastDue" style="font-size:13px;margin-top:4px;opacity:.7">
            Volgende factuur: {{ formatDate(status.nextBillingDate) }}
          </div>
        </div>
        <button class="btn btn-primary" :disabled="loading" @click="manage">
          {{ isPastDue ? 'Betalingsgegevens bijwerken' : 'Abonnement beheren' }}
        </button>
      </template>

      <!-- Free / Canceled -->
      <template v-else>
        <div>
          <div style="font-weight:600;font-size:1.05rem">
            Gratis<span v-if="isCanceled" style="opacity:.6;font-weight:400"> (abonnement opgezegd)</span>
          </div>
          <div style="margin-top:.75rem;display:flex;flex-direction:column;gap:4px;font-size:14px">
            <div>✓ 1 gebruiker</div>
            <div>✓ 1 API-sleutel</div>
          </div>
        </div>
        <button class="btn btn-primary" :disabled="loading" @click="upgrade">
          {{ loading ? 'Bezig...' : (isCanceled ? 'Opnieuw upgraden naar Pro' : 'Upgrade naar Pro') }}
        </button>
      </template>
    </div>
  </div>
</template>
```

- [ ] **Stap 3: Maak billing/success.vue aan**

```vue
<!-- frontend/pages/billing/success.vue -->
<script setup lang="ts">
definePageMeta({ middleware: 'auth' })
</script>

<template>
  <div style="max-width:480px;text-align:center;padding:3rem 0">
    <div style="font-size:2.5rem;margin-bottom:1rem">🎉</div>
    <div class="content-title" style="margin-bottom:.5rem">Welkom bij Pro!</div>
    <div style="opacity:.7;margin-bottom:1.5rem">Je abonnement is actief. Alle limieten zijn opgeheven.</div>
    <NuxtLink to="/dashboard" class="btn btn-primary">Naar dashboard</NuxtLink>
  </div>
</template>
```

- [ ] **Stap 4: Voeg billing toe aan de sidebar navigatie**

Open `frontend/components/AppSidebar.vue` en voeg een nav-item toe voor `/billing` (in de sectie met tenant-links, niet SuperAdmin):

```html
<NuxtLink to="/billing" class="nav-item">
  <span class="nav-icon">💳</span>
  Abonnement
</NuxtLink>
```

- [ ] **Stap 5: Commit**

```bash
git add frontend/composables/useBilling.ts \
        frontend/pages/billing.vue \
        frontend/pages/billing/success.vue \
        frontend/components/AppSidebar.vue
git commit -m "feat: voeg billing pagina toe met Stripe Checkout en Customer Portal"
```

---

## Stripe testen met Stripe CLI

Na de implementatie test je de volledige flow lokaal:

**1. Installeer Stripe CLI (eenmalig):**
```bash
brew install stripe/stripe-cli/stripe
```

**2. Login op Stripe:**
```bash
stripe login
```

**3. Start webhook forwarding naar je lokale server:**
```bash
stripe listen --forward-to localhost:5000/api/billing/webhook
```

De CLI toont je een webhook secret (`whsec_...`). Vul dit in als `Stripe:WebhookSecret` in je lokale omgevingsvariabelen.

**4. Zet je Stripe test keys in omgevingsvariabelen (nooit in appsettings.json):**
```bash
export Stripe__SecretKey="sk_test_..."
export Stripe__ProPriceId="price_..."
export Stripe__WebhookSecret="whsec_..."
```

**5. Doorloop de flow:**
- Registreer via `/register`
- Ga naar `/billing` → klik "Upgrade naar Pro"
- Gebruik Stripe testnummer: `4242 4242 4242 4242`, vervaldatum in de toekomst, willekeurige CVC
- Verifieer dat `/billing` nu "Pro ● Actief" toont
