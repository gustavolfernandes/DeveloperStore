using Ambev.DeveloperEvaluation.Application.Users.GetUser;
using AutoMapper;

namespace Ambev.DeveloperEvaluation.WebApi.Features.Users.GetUser;

/// <summary>
/// Profile mapping the GetUser application result to the API response.
/// </summary>
public class GetUserProfile : Profile
{
    public GetUserProfile()
    {
        CreateMap<GetUserResult, GetUserResponse>();
    }
}
