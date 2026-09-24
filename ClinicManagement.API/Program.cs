using ClinicManagement.API.Middleware;
using ClinicManagement.API.Services;
using ClinicManagement.Application;
using ClinicManagement.Application.Common.Interfaces;
using ClinicManagement.Infrastructure;
using ClinicManagement.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add Application and Infrastructure services
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Add HTTP Context and Current User services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// Add Controllers
builder.Services.AddControllers();

// Configure OpenAPI (.NET 10 native OpenAPI support)
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info = new OpenApiInfo
        {
            Title = "Clinic Management API",
            Version = "v1",
            Description = "Production-grade Clinic Management System Web API built with .NET 10 and Clean Architecture."
        };

        var securityScheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description = "Enter JWT Bearer token: Bearer {token}",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes.Add("Bearer", securityScheme);

        document.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }] = Array.Empty<string>()
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Centralized Exception Handling Middleware
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure OpenAPI & Scalar
app.MapOpenApi("/openapi/v1.json");
app.MapScalarApiReference("/scalar", options =>
{
    options.WithTitle("Clinic Management API Documentation");
    options.WithTheme(ScalarTheme.Moon);
    options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
});

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Automatic DB Migration and Seeding on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var context = services.GetRequiredService<ClinicDbContext>();
        if (context.Database.IsRelational())
        {
            await context.Database.MigrateAsync();
        }

        var seeder = services.GetRequiredService<ClinicDbSeeder>();
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred during database migration/seeding on startup.");
    }
}

app.Run();

public partial class Program { }
