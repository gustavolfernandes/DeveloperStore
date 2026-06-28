using Ambev.DeveloperEvaluation.WebApi;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;
using Xunit;

namespace Ambev.DeveloperEvaluation.Functional;

/// <summary>
/// Hosts the API in-memory and points it at a disposable PostgreSQL container, so functional
/// tests run against the full HTTP pipeline (routing, authentication, middleware, mapping) and
/// a real database. The schema is created by the application's startup migrations.
/// </summary>
public sealed class FunctionalApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer? _database;

    /// <summary>
    /// Whether a Docker daemon was reachable to start the database container. When false the
    /// functional tests skip themselves instead of failing.
    /// </summary>
    public bool DockerAvailable { get; private set; }

    public FunctionalApiFactory()
    {
        Environment.SetEnvironmentVariable("Jwt__SecretKey", "functional-tests-signing-key-not-used-anywhere-else");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _database!.GetConnectionString()
            });
        });
    }

    public async Task InitializeAsync()
    {
        try
        {
            _database = new PostgreSqlBuilder().WithImage("postgres:13").Build();
            await _database.StartAsync();
            DockerAvailable = true;
        }
        catch
        {
            DockerAvailable = false;
        }
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        if (_database is not null)
            await _database.DisposeAsync();
        await base.DisposeAsync();
    }
}
