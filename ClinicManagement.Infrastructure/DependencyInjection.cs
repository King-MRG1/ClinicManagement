using System.Text;
using ClinicManagement.Application.Abstractions;
using ClinicManagement.Application.Interfaces;
using ClinicManagement.Infrastructure.Persistence;
using ClinicManagement.Domain.Entities;
using ClinicManagement.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ClinicManagement.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<ClinicDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Register the interface for dependency injection
        services.AddScoped<IClinicDbContext>(provider => provider.GetRequiredService<ClinicDbContext>());
        services.AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.User.RequireUniqueEmail = true;
        })
        .AddEntityFrameworkStores<ClinicDbContext>()
        .AddDefaultTokenProviders();

        var secret = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new InvalidOperationException("Configuration 'Jwt:Key' is missing.");
        }
        if (secret.Equals("PLACEHOLDER_SET_IN_USER_SECRETS_OR_ENVIRONMENT_VARIABLES", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Configuration 'Jwt:Key' is set to placeholder text. Please configure a valid key using dotnet user-secrets or environment variables.");
        }
        if (secret.Length < 32)
        {
            throw new InvalidOperationException("Configuration 'Jwt:Key' must be at least 32 characters long.");
        }

        var issuer = configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("Configuration 'Jwt:Issuer' is missing.");
        }

        var audience = configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Configuration 'Jwt:Audience' is missing.");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                ClockSkew = TimeSpan.Zero
            };
        });

        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
