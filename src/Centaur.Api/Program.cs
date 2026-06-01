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
const string CorsPolicy = "CentaurAdmin";

builder.Services.AddDbContext<CentaurDbContext>(opts =>
    opts.UseNpgsql(connectionString));

builder.Services.AddCors(opts =>
{
    opts.AddPolicy(CorsPolicy, policy =>
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddScoped<TenantSchemaHelper>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IApiKeyRepository, ApiKeyRepository>();
builder.Services.AddScoped<IWebsiteSettingsRepository, WebsiteSettingsRepository>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IApiKeyService, ApiKeyService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IWebsiteSettingsService, WebsiteSettingsService>();
builder.Services.AddScoped<IAuthService>(_ => new AuthService(
    _.GetRequiredService<IUserRepository>(), jwtSecret));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.MapInboundClaims = false;
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

// Seed SuperAdmin bij eerste opstart
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CentaurDbContext>();
    await db.Database.MigrateAsync();

    var superAdminEmail = builder.Configuration["SuperAdmin:Email"] ?? "superadmin@centaur.nl";
    var superAdminPassword = builder.Configuration["SuperAdmin:Password"]!;

    if (!await db.Users.AnyAsync(u => u.Role == Centaur.Domain.Enums.UserRole.SuperAdmin))
    {
        db.Users.Add(new Centaur.Domain.Entities.User
        {
            Id = Guid.NewGuid(),
            Email = superAdminEmail,
            PasswordHash = await Task.Run(() => BCrypt.Net.BCrypt.HashPassword(superAdminPassword)),
            Role = Centaur.Domain.Enums.UserRole.SuperAdmin,
            TenantId = null,
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Centaur.Api.Middleware.TenantSchemaMiddleware>();
app.MapControllers();
app.Run();

public partial class Program { }
