using AutoMapper;
using SaasSystem.Customers;
using SaasSystem.Domain.Entities;
using SaasSystem.Tasks;

namespace SaasSystem;

public class SaasSystemApplicationAutoMapperProfile : Profile
{
    public SaasSystemApplicationAutoMapperProfile()
    {
        CreateMap<Customer, CustomerDto>();
        CreateMap<WorkTask, KanbanTaskCardDto>();
    }
}
