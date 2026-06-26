using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Ambev.DeveloperEvaluation.Infrastructure.Mapping;

public class SaleItemConfiguration : IEntityTypeConfiguration<SaleItem>
{
    public void Configure(EntityTypeBuilder<SaleItem> builder)
    {
        builder.ToTable("SaleItems");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new SaleItemId(value))
            .ValueGeneratedNever();

        builder.Property(i => i.SaleId)
            .HasConversion(id => id.Value, value => new SaleId(value))
            .IsRequired();
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitPrice).HasColumnType("numeric(18,2)");
        builder.Property(i => i.DiscountRate).HasColumnType("numeric(5,4)");
        builder.Property(i => i.Discount).HasColumnType("numeric(18,2)");
        builder.Property(i => i.Total).HasColumnType("numeric(18,2)");
        builder.Property(i => i.IsCancelled).IsRequired();

        builder.OwnsOne(i => i.Product, owned =>
        {
            owned.Property(p => p.Id).HasColumnName("ProductId").IsRequired();
            owned.Property(p => p.Name).HasColumnName("ProductName").IsRequired().HasMaxLength(150);
        });
        builder.Navigation(i => i.Product).IsRequired();

        builder.Ignore(i => i.GrossAmount);
    }
}
