using Ambev.DeveloperEvaluation.Application;
using Ambev.DeveloperEvaluation.Infrastructure;
using Ambev.DeveloperEvaluation.WebApi.Endpoints;
using Ambev.DeveloperEvaluation.WebApi.Extensions;
using Ambev.DeveloperEvaluation.WebApi.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;

namespace Ambev.DeveloperEvaluation.WebApi;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");

            var builder = WebApplication.CreateBuilder(args);
            builder.AddDefaultLogging();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(ConfigureSwaggerAuth);
            builder.AddBasicHealthChecks();

            builder.Services.AddInfrastructure(builder.Configuration);
            builder.Services.AddApplication();
            builder.Services.AddJwtAuthentication(builder.Configuration);

            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(ApplicationLayer).Assembly);

            var app = builder.Build();

            ApplyMigrations(app);

            app.UseMiddleware<ValidationExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseBasicHealthChecks();
            app.UseDefaultLogging();

            app.MapAuthEndpoints();
            app.MapUserEndpoints();
            app.MapSaleEndpoints();

            app.Run();
        }
        catch (Exception ex) when (ex.GetType().Name is not ("HostAbortedException" or "StopTheHostException"))
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    /// Adds a JWT Bearer security definition to Swagger so protected endpoints can be
    /// called from the Swagger UI using the "Authorize" button.
    /// </summary>
    private static void ConfigureSwaggerAuth(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        var scheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter the JWT token obtained from POST /api/auth (without the 'Bearer ' prefix).",
            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
        };

        options.AddSecurityDefinition("Bearer", scheme);
        options.AddSecurityRequirement(new OpenApiSecurityRequirement { [scheme] = Array.Empty<string>() });
    }

    /// <summary>
    /// Applies pending EF Core migrations on startup, retrying while the database
    /// container is still warming up.
    /// </summary>
    private static void ApplyMigrations(WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<DefaultContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        const int maxAttempts = 10;
        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                context.Database.Migrate();
                logger.LogInformation("Database migrations applied successfully.");
                return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(ex, "Database not ready (attempt {Attempt}/{Max}). Retrying in 3s...", attempt, maxAttempts);
                Thread.Sleep(TimeSpan.FromSeconds(3));
            }
        }

        context.Database.Migrate();
    }
}
