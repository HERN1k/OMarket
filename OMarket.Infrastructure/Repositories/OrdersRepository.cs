using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;
using OMarket.Domain.Enums;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.StaticCollections;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class OrdersRepository : IOrdersRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly IStaticCollectionsService _staticCollections;

        private readonly ILogger<OrdersRepository> _logger;

        public OrdersRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                IStaticCollectionsService staticCollections,
                ILogger<OrdersRepository> logger
            )
        {
            _contextFactory = contextFactory;
            _staticCollections = staticCollections;
            _logger = logger;
        }

        public async Task<OrderDto?> SaveNewOrderAsync(CreatedOrderDto orderDto, CancellationToken token)
        {
            if (orderDto is null ||
                orderDto.Products.Count <= 0 ||
                orderDto.StoreId == Guid.Empty ||
                string.IsNullOrEmpty(orderDto.Comment) ||
                orderDto.TotalPrice <= 0.0M ||
                orderDto.TotalQuantity <= 0 ||
                orderDto.DeliveryMethod == DeliveryMethod.NONE)
            {
                return null;
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                OrderStatus? orderStatus = await context.OrderStatuses
                    .Where(status => status.Status == "Взято в обробку")
                    .SingleOrDefaultAsync(token);

                if (orderStatus is null)
                {
                    return null;
                }

                string deliveryMethod = orderDto.DeliveryMethod == DeliveryMethod.DELIVERY
                    ? "Доставка"
                    : "Самовивіз";

                Order order = new()
                {
                    Id = orderDto.Id,
                    CustomerId = orderDto.CustomerId,
                    StoreId = orderDto.StoreId,
                    TotalAmount = orderDto.TotalPrice,
                    OrderStatus = orderStatus,
                    DeliveryMethod = deliveryMethod
                };

                await context.Orders.AddAsync(order, token);

                List<OrderItem> orderItems = new();

                foreach (var item in orderDto.Products)
                {
                    decimal price = item.Quantity * item.Product.Price;

                    orderItems.Add(new OrderItem()
                    {
                        Order = order,
                        ProductId = item.Product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = item.Product.Price,
                        TotalPrice = price
                    });
                }

                await context.OrderItems.AddRangeAsync(orderItems, token);

                await context.SaveChangesAsync(token);

                OrderDto newOrderDto = new()
                {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    StoreId = order.StoreId,
                    TotalAmount = order.TotalAmount,
                    StatusId = order.StatusId,
                    Status = orderStatus.Status,
                    CreatedAt = order.CreatedAt
                };

                foreach (var item in orderItems)
                {
                    newOrderDto.Products.Add(new OrderItemDto()
                    {
                        Id = item.Id,
                        OrderId = item.OrderId,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    });
                }

                if (orderItems.Count != newOrderDto.Products.Count)
                {
                    return null;
                }

                return newOrderDto;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TelegramException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public async Task ChangeOrderStatusAsync(string status, Guid orderId, CancellationToken token)
        {
            if (string.IsNullOrEmpty(status) || orderId == Guid.Empty)
            {
                throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                OrderStatus statusEntity = await context.OrderStatuses
                    .Where(e => e.Status == status)
                    .SingleOrDefaultAsync(token)
                        ?? throw new TelegramException();

                Order order = await context.Orders
                    .Where(e => e.Id == orderId)
                    .SingleOrDefaultAsync(token)
                        ?? throw new TelegramException();

                order.OrderStatus = statusEntity;

                await context.SaveChangesAsync(token);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TelegramException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public async Task<List<ViewOrderDto>> GetLastCustomerOrdersAsync(long id, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                List<ViewOrderDto> orders = await GetCustomerOrdersAsync(context, id, token);

                if (orders.Count <= 0)
                {
                    return new();
                }

                List<Guid> orderIds = orders
                    .Select(order => order.Id)
                    .ToList();

                List<ViewOrderItemDto> allOrderItems = await GetOrderItemsAsync(context, orderIds, token);

                if (allOrderItems.Count <= 0)
                {
                    return new();
                }

                AttachProductsToOrders(orders, allOrderItems);

                AttachStatusAndStoreInfo(orders);

                return orders
                    .OrderBy(order => order.CreatedAt)
                    .ToList();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (TelegramException)
            {
                return new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                return new();
            }
        }

        private async Task<List<ViewOrderDto>> GetCustomerOrdersAsync(AppDBContext context, long customerId, CancellationToken token)
        {
            return await context.Orders
                .AsNoTracking()
                .Where(order => order.CustomerId == customerId)
                .OrderByDescending(order => order.CreatedAt)
                .Take(3)
                .Select(order => new ViewOrderDto
                {
                    Id = order.Id,
                    TotalAmount = order.TotalAmount,
                    StatusId = order.StatusId,
                    StoreId = order.StoreId,
                    DeliveryMethod = order.DeliveryMethod,
                    CreatedAt = order.CreatedAt,
                })
                .ToListAsync(token);
        }

        private async Task<List<ViewOrderItemDto>> GetOrderItemsAsync(AppDBContext context, List<Guid> orderIds, CancellationToken token)
        {
            return await context.OrderItems
                .AsNoTracking()
                .Where(item => orderIds.Contains(item.OrderId))
                .Select(item => new ViewOrderItemDto
                {
                    Id = item.Id,
                    OrderId = item.OrderId,
                    Quantity = item.Quantity,
                    TotalPrice = item.TotalPrice,
                    ProductId = item.ProductId
                })
                .ToListAsync(token);
        }

        private void AttachProductsToOrders(List<ViewOrderDto> orders, List<ViewOrderItemDto> allOrderItems)
        {
            foreach (var item in allOrderItems)
            {
                ViewOrderDto order = orders
                    .SingleOrDefault(order => order.Id == item.OrderId)
                        ?? throw new TelegramException();

                ProductFullNameWithPrice? product = _staticCollections.ProductGuidToFullNameWithPriceDictionary
                    .GetValueOrDefault(item.ProductId);

                if (product is not null)
                {
                    item.FullName = product.FullName;
                    item.Price = product.Price;
                }
                else
                {
                    item.FullName = "Unknown product";
                    item.Price = decimal.Zero;
                }

                order.Products.Add(item);
                order.TotalQuantity += item.Quantity;
            }
        }

        private void AttachStatusAndStoreInfo(List<ViewOrderDto> orders)
        {
            foreach (var item in orders)
            {
                item.Status = _staticCollections.OrderStatusesWithGuidDictionary
                    .GetValueOrDefault(item.StatusId) ?? throw new TelegramException();

                item.Store = _staticCollections.CitiesWithStoreAddressesDictionary
                    .Where(e => e.Value.StoreId == item.StoreId)
                    .SingleOrDefault()
                    .Value ?? throw new TelegramException();
            }
        }
    }
}