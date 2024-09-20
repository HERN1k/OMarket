using AutoMapper;

using OMarket.Domain.Entities;

using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace OMarket.Domain.Mapper
{
    public class ToCustomerMapper : ITypeConverter<Update, Customer>
    {
        public Customer Convert(Update source, Customer destination, ResolutionContext context)
        {
            User user;
            if (source.Type == UpdateType.Message)
            {
                if (source.Message is null ||
                    source.Message.From is null)
                {
                    throw new AutoMapperMappingException($"Error '{nameof(Update)}' to '{nameof(Customer)}' mapping.");
                }

                user = source.Message.From;
            }
            else if (source.Type == UpdateType.CallbackQuery)
            {
                if (source.CallbackQuery is null ||
                    source.CallbackQuery.From is null)
                {
                    throw new AutoMapperMappingException($"Error '{nameof(Update)}' to '{nameof(Customer)}' mapping.");
                }

                user = source.CallbackQuery.From;
            }
            else
            {
                throw new AutoMapperMappingException($"Error '{nameof(Update)}' to '{nameof(Customer)}' mapping.");
            }

            return new Customer()
            {
                Id = user.Id,
                Username = user.Username,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsBot = user.IsBot,
            };
        }
    }
}