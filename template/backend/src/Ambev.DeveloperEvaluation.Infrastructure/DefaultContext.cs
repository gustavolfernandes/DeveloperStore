using System.Reflection;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Ambev.DeveloperEvaluation.Infrastructure;

public class DefaultContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Sale> Sales { get; set; }

    public DefaultContext(DbContextOptions<DefaultContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}

/// <summary>
/// Design-time factory used by the EF Core tools (e.g. <c>dotnet ef migrations add</c>).
/// It only needs the provider configured to build the model; no database connection is
/// opened when adding migrations. At runtime the real connection string is supplied by
/// <see cref="DependencyInjection.AddInfrastructure"/>.
/// </summary>
public class DefaultContextFactory : IDesignTimeDbContextFactory<DefaultContext>
{
    public DefaultContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<DefaultContext>();
        builder.UseNpgsql(
            "Host=localhost;Database=DeveloperEvaluation;Username=postgres;Password=postgres",
            b => b.MigrationsAssembly(typeof(DefaultContext).Assembly.FullName));

        return new DefaultContext(builder.Options);
    }
}
