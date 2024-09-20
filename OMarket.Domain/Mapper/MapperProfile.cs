using AutoMapper;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;

using Telegram.Bot.Types;

namespace OMarket.Domain.Mapper
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<Update, Customer>().ConvertUsing<ToCustomerMapper>();
            CreateMap<Update, CustomerDto>().ConvertUsing<ToCustomerDtoMapper>();
        }
    }
}