using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

using Telegram.Bot.Types;

namespace OMarket.Infrastructure.Repositories
{
    public class CustomersRepository : ICustomersRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<CustomersRepository> _logger;

        private readonly ICacheService _cache;

        private readonly IMapper _mapper;

        public CustomersRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                ILogger<CustomersRepository> logger,
                ICacheService cache,
                IMapper mapper
            )
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<CustomerDto> GetCustomerFromIdAsync(long id, CancellationToken token)
        {
            CustomerDto? customer;

            string cacheKey = $"{CacheKeys.CustomerDtoId}{id}";

            customer = await _cache.GetCacheAsync<CustomerDto>(cacheKey);

            if (customer is null)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                    Customer customerEntity = await context.Customers
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == id, cancellationToken: token)
                            ?? throw new TelegramException("exception_first_use_command_start");

                    customer = new CustomerDto()
                    {
                        Id = customerEntity.Id,
                        Username = customerEntity.Username,
                        FirstName = customerEntity.FirstName,
                        LastName = customerEntity.LastName,
                        PhoneNumber = customerEntity.PhoneNumber,
                        IsBot = customerEntity.IsBot,
                        StoreId = customerEntity.StoreId,
                        CreatedAt = customerEntity.CreatedAt,
                        BlockedOrders = customerEntity.BlockedOrders,
                        BlockedReviews = customerEntity.BlockedReviews,
                    };

                    await _cache.SetCacheAsync(cacheKey, customer);
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

            if (customer is null)
            {
                throw new TelegramException();
            }

            return customer;
        }

        public async Task<CustomerDto> GetCustomerFromIdAsync(long id)
        {
            CustomerDto? customer;

            string cacheKey = $"{CacheKeys.CustomerDtoId}{id}";

            customer = await _cache.GetCacheAsync<CustomerDto>(cacheKey);

            if (customer is null)
            {
                try
                {
                    await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

                    Customer customerEntity = await context.Customers
                        .AsNoTracking()
                        .SingleOrDefaultAsync(e => e.Id == id)
                            ?? throw new TelegramException("exception_first_use_command_start");

                    customer = new CustomerDto()
                    {
                        Id = customerEntity.Id,
                        Username = customerEntity.Username,
                        FirstName = customerEntity.FirstName,
                        LastName = customerEntity.LastName,
                        PhoneNumber = customerEntity.PhoneNumber,
                        IsBot = customerEntity.IsBot,
                        StoreId = customerEntity.StoreId,
                        CreatedAt = customerEntity.CreatedAt,
                        BlockedOrders = customerEntity.BlockedOrders,
                        BlockedReviews = customerEntity.BlockedReviews,
                    };

                    await _cache.SetCacheAsync(cacheKey, customer);
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

            if (customer is null)
            {
                throw new TelegramException();
            }

            return customer;
        }

        public async Task<bool> AnyCustomerByIdAsync(long id, CancellationToken token)
        {
            CustomerDto? customer = await _cache.GetCacheAsync<CustomerDto>($"{CacheKeys.CustomerDtoId}{id}");

            if (customer is null)
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                    return await context.Customers
                        .AsNoTracking()
                        .AnyAsync(e => e.Id == id, cancellationToken: token);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Message}", ex.Message);
                    throw;
                }
            }
            else
            {
                return true;
            }
        }

        public async Task<CustomerDto> AddNewCustomerAsync(Update update, CancellationToken token)
        {
            Customer customer = _mapper.Map<Customer>(update);

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                await context.Customers.AddAsync(customer, cancellationToken: token);

                await context.SaveChangesAsync(token);

                CustomerDto customerDto = new()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    IsBot = customer.IsBot,
                    StoreId = customer.StoreId,
                    CreatedAt = customer.CreatedAt,
                    BlockedOrders = customer.BlockedOrders,
                    BlockedReviews = customer.BlockedReviews,
                };

                await _cache.SetCacheAsync($"{CacheKeys.CustomerDtoId}{customer.Id}", customerDto);

                return customerDto;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }

        public async Task SaveContactsAsync(long id, string phoneNumber, CancellationToken token, string? firstName = null, string? lastName = null)
        {
            if (string.IsNullOrEmpty(phoneNumber) || string.IsNullOrWhiteSpace(phoneNumber))
            {
                throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                Customer customer = await context.Customers
                    .SingleOrDefaultAsync(c => c.Id == id, cancellationToken: token)
                        ?? throw new TelegramException("exception_first_use_command_start");

                if (!string.IsNullOrEmpty(firstName))
                {
                    customer.FirstName = firstName;
                }

                if (!string.IsNullOrEmpty(lastName))
                {
                    customer.LastName = lastName;
                }

                customer.PhoneNumber = phoneNumber;

                await context.SaveChangesAsync(token);

                CustomerDto customerDto = new CustomerDto()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = phoneNumber,
                    IsBot = customer.IsBot,
                    StoreId = customer.StoreId,
                    CreatedAt = customer.CreatedAt
                };

                await _cache.SetCacheAsync($"{CacheKeys.CustomerDtoId}{customer.Id}", customerDto);
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

        public async Task SaveStoreAsync(long id, Guid storeId, CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                Customer customer = await context.Customers
                    .SingleOrDefaultAsync(e => e.Id == id, cancellationToken: token)
                        ?? throw new TelegramException("exception_first_use_command_start");

                if (storeId == Guid.Empty)
                {
                    throw new TelegramException();
                }

                Store store = await context.Stores
                    .SingleOrDefaultAsync(store => store.Id == storeId, cancellationToken: token)
                        ?? throw new TelegramException();

                customer.Store = store;

                await context.SaveChangesAsync(token);

                CustomerDto customerDto = new()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    IsBot = customer.IsBot,
                    StoreId = storeId,
                    CreatedAt = customer.CreatedAt
                };

                await _cache.SetCacheAsync($"{CacheKeys.CustomerDtoId}{customer.Id}", customerDto);
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

        public async Task RemoveCustomerAsync(long id)
        {
            try
            {
                await using AppDBContext context = await _contextFactory.CreateDbContextAsync();

                Customer? customer = await context.Customers
                    .SingleOrDefaultAsync(e => e.Id == id);

                if (customer is null)
                {
                    return;
                }

                context.Customers.Remove(customer);

                await context.SaveChangesAsync();

                await _cache.RemoveCacheAsync($"{CacheKeys.CustomerDtoId}{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }
    }
}