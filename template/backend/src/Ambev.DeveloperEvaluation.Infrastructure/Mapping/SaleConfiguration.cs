using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.Infrastructure.Mapping;

public class SaleConfiguration : IEntityTypeConfiguration<Sale>
{
    public void Configure(EntityTypeBuilder<Sale> builder)
    {
        builder.ToTable("Sales");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, value => new SaleId(value))
            .ValueGeneratedNever();

        builder.Property(s => s.SaleNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.SaleNumber).IsUnique();

        builder.Property(s => s.SaleDate).IsRequired();
        builder.Property(s => s.TotalAmount).HasColumnType("numeric(18,2)");
        builder.Property(s => s.IsCancelled).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();
        builder.Property(s => s.UpdatedAt);

        builder.OwnsOne(s => s.Customer, owned =>
        {
            owned.Property(p => p.Id).HasColumnName("CustomerId").IsRequired();
            owned.Property(p => p.Name).HasColumnName("CustomerName").IsRequired().HasMaxLength(100);
        });
        builder.Navigation(s => s.Customer).IsRequired();

        builder.OwnsOne(s => s.Branch, owned =>
        {
            owned.Property(p => p.Id).HasColumnName("BranchId").IsRequired();
            owned.Property(p => p.Name).HasColumnName("BranchName").IsRequired().HasMaxLength(100);
        });
        builder.Navigation(s => s.Branch).IsRequired();

        var itemsNavigation = builder.Metadata.FindNavigation(nameof(Sale.Items))!;
        itemsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        builder.HasMany(s => s.Items)
            .WithOne()
            .HasForeignKey(i => i.SaleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
