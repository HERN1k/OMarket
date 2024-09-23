using OMarket.Domain.DTOs;

using Telegram.Bot.Types;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface ICustomersRepository
    {
        Task<CustomerDto> GetCustomerFromIdAsync(long id, CancellationToken token);

        Task<CustomerDto> GetCustomerFromIdAsync(long id);

        Task<bool> AnyCustomerByIdAsync(long id, CancellationToken token);

        Task<CustomerDto> AddNewCustomerAsync(Update update, CancellationToken token);

        Task SaveContactsAsync(long id, string phoneNumber, CancellationToken token, string? firstName = null, string? lastName = null);

        Task SaveStoreAddressAsync(long id, string city, string address, CancellationToken token);

        Task RemoveCustomerAsync(long id);


    }
}