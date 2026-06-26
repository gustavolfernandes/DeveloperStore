using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Sales.Common;

/// <summary>AutoMapper profile mapping the Sale aggregate to its result DTOs.</summary>
public class SaleMappingProfile : Profile
{
    public SaleMappingProfile()
    {
        CreateMap<ExternalIdentity, ExternalIdentityDto>();
        CreateMap<SaleItem, SaleItemResult>();
        CreateMap<Sale, SaleResult>();
    }
}
