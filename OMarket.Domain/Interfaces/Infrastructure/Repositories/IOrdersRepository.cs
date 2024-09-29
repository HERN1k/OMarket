using OMarket.Domain.DTOs;

namespace OMarket.Domain.Interfaces.Infrastructure.Repositories
{
    public interface IOrdersRepository
    {
        Task<OrderDto?> SaveNewOrderAsync(CreatedOrderDto order, CancellationToken token);

        Task ChangeOrderStatusAsync(string status, Guid orderId, CancellationToken token);

        Task<List<ViewOrderDto>> GetLastCustomerOrdersAsync(long id, CancellationToken token);
    }
}