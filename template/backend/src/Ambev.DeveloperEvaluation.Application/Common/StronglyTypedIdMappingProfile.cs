using Ambev.DeveloperEvaluation.Domain.ValueObjects;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.Application.Common;

/// <summary>
/// AutoMapper conversions between the strongly-typed identifiers and their underlying
/// <see cref="Guid"/> values, so domain entities can be projected onto the primitive-keyed
/// result DTOs (and vice versa) without per-member configuration.
/// </summary>
public sealed class StronglyTypedIdMappingProfile : Profile
{
    public StronglyTypedIdMappingProfile()
    {
        CreateMap<SaleId, Guid>().ConvertUsing(id => id.Value);
        CreateMap<Guid, SaleId>().ConvertUsing(value => new SaleId(value));

        CreateMap<SaleItemId, Guid>().ConvertUsing(id => id.Value);
        CreateMap<Guid, SaleItemId>().ConvertUsing(value => new SaleItemId(value));

        CreateMap<UserId, Guid>().ConvertUsing(id => id.Value);
        CreateMap<Guid, UserId>().ConvertUsing(value => new UserId(value));
    }
}
