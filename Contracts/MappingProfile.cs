using AutoMapper;
using Contracts.Client.Commands.Models;
using Contracts.Client.Queries.Models;

namespace Contracts;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<Persistence.Entities.Client, GetClientResponseModel>().ReverseMap();
        CreateMap<CreateClientRequestModel, Persistence.Entities.Client>().ReverseMap();
        CreateMap<UpdateClientRequestModel, Persistence.Entities.Client>().ReverseMap();
        CreateMap<Persistence.Entities.Client, UpdateClientResponseModel>().ReverseMap();
    }
}