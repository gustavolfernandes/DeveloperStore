using Ambev.DeveloperEvaluation.Domain.Services;
using FluentValidation;

namespace Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;

public class UpdateSaleValidator : AbstractValidator<UpdateSaleCommand>
{
    public UpdateSaleValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.SaleDate).NotEmpty().WithMessage("Sale date is required.");
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.BranchName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Items).NotEmpty().WithMessage("A sale must contain at least one item.");
        RuleForEach(x => x.Items).SetValidator(new UpdateSaleItemCommandValidator());
    }
}

public class UpdateSaleItemCommandValidator : AbstractValidator<UpdateSaleItemCommand>
{
    public UpdateSaleItemCommandValidator()
    {
        RuleFor(i => i.ProductId).NotEmpty();
        RuleFor(i => i.ProductName).NotEmpty().MaximumLength(150);
        RuleFor(i => i.Quantity)
            .GreaterThan(0)
            .LessThanOrEqualTo(SaleDiscountPolicy.MaxQuantityPerItem)
            .WithMessage($"It is not possible to sell more than {SaleDiscountPolicy.MaxQuantityPerItem} identical items.");
        RuleFor(i => i.UnitPrice).GreaterThanOrEqualTo(0);
    }
}
