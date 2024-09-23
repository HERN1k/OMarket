using System.Text.Json;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;
using OMarket.Domain.Exceptions.Telegram;
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

        private readonly IDistributedCache _cache;

        private readonly IMapper _mapper;

        public CustomersRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                ILogger<CustomersRepository> logger,
                IDistributedCache cache,
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
            if (id <= 0 || id > long.MaxValue)
            {
                throw new TelegramException();
            }
            CustomerDto? customer;

            string? customerString = await _cache.GetStringAsync($"{CacheKeys.CustomerDtoId}{id}", token);

            if (string.IsNullOrEmpty(customerString))
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
                        CityId = customerEntity.CityId,
                        IsBot = customerEntity.IsBot,
                        StoreAddressId = customerEntity.StoreAddressId,
                        CreatedAt = customerEntity.CreatedAt
                    };

                    customerString = JsonSerializer.Serialize<CustomerDto>(customer);

                    if (string.IsNullOrEmpty(customerString))
                    {
                        throw new TelegramException();
                    }

                    await _cache.SetStringAsync($"{CacheKeys.CustomerDtoId}{id}", customerString, token);
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
            else
            {
                token.ThrowIfCancellationRequested();

                customer = JsonSerializer.Deserialize<CustomerDto>(customerString);
            }

            if (customer is null)
            {
                throw new TelegramException();
            }

            return customer;
        }

        public async Task<CustomerDto> GetCustomerFromIdAsync(long id)
        {
            if (id <= 0 || id > long.MaxValue)
            {
                throw new TelegramException();
            }
            CustomerDto? customer;

            string? customerString = await _cache.GetStringAsync($"{CacheKeys.CustomerDtoId}{id}");

            if (string.IsNullOrEmpty(customerString))
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
                        CityId = customerEntity.CityId,
                        IsBot = customerEntity.IsBot,
                        StoreAddressId = customerEntity.StoreAddressId,
                        CreatedAt = customerEntity.CreatedAt
                    };

                    customerString = JsonSerializer.Serialize<CustomerDto>(customer);

                    if (string.IsNullOrEmpty(customerString))
                    {
                        throw new TelegramException();
                    }

                    await _cache.SetStringAsync($"{CacheKeys.CustomerDtoId}{id}", customerString);
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
            else
            {
                customer = JsonSerializer.Deserialize<CustomerDto>(customerString);
            }

            if (customer is null)
            {
                throw new TelegramException();
            }

            return customer;
        }

        public async Task<bool> AnyCustomerByIdAsync(long id, CancellationToken token)
        {
            if (id <= 0 || id > long.MaxValue)
            {
                throw new TelegramException();
            }

            if (string.IsNullOrEmpty(await _cache.GetStringAsync($"{CacheKeys.CustomerDtoId}{id}", token)))
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

                CustomerDto customerDto = new CustomerDto()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    CityId = customer.CityId,
                    IsBot = customer.IsBot,
                    StoreAddressId = customer.StoreAddressId,
                    CreatedAt = customer.CreatedAt
                };

                string? customerString = JsonSerializer.Serialize<CustomerDto>(customerDto);

                if (string.IsNullOrEmpty(customerString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync($"{CacheKeys.CustomerDtoId}{customer.Id}", customerString, token);

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
            if (id <= 0 || id > long.MaxValue)
            {
                throw new TelegramException();
            }

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
                    PhoneNumber = customer.PhoneNumber,
                    CityId = customer.CityId,
                    IsBot = customer.IsBot,
                    StoreAddressId = customer.StoreAddressId,
                    CreatedAt = customer.CreatedAt
                };

                string? customerString = JsonSerializer.Serialize<CustomerDto>(customerDto);

                if (string.IsNullOrEmpty(customerString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync($"{CacheKeys.CustomerDtoId}{customer.Id}", customerString, token);
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

        public async Task SaveStoreAddressAsync(long id, string city, string address, CancellationToken token)
        {
            if (id <= 0 || id > long.MaxValue)
            {
                throw new TelegramException();
            }

            if (string.IsNullOrEmpty(city) || string.IsNullOrEmpty(address))
            {
                throw new TelegramException();
            }

            try
            {
                token.ThrowIfCancellationRequested();

                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                Customer customer = await context.Customers
                    .SingleOrDefaultAsync(e => e.Id == id, cancellationToken: token)
                        ?? throw new TelegramException("exception_first_use_command_start");

                City cityEntity = await context.Cities
                    .SingleOrDefaultAsync(e => e.CityName == city, cancellationToken: token)
                        ?? throw new TelegramException();

                StoreAddress addressEntity = await context.StoreAddresses
                    .SingleOrDefaultAsync(e => e.Address == address, cancellationToken: token)
                        ?? throw new TelegramException();

                customer.City = cityEntity;
                customer.StoreAddress = addressEntity;

                await context.SaveChangesAsync(token);

                string? customerString = JsonSerializer.Serialize<CustomerDto>(new CustomerDto()
                {
                    Id = customer.Id,
                    Username = customer.Username,
                    FirstName = customer.FirstName,
                    LastName = customer.LastName,
                    PhoneNumber = customer.PhoneNumber,
                    CityId = customer.CityId,
                    IsBot = customer.IsBot,
                    StoreAddressId = customer.StoreAddressId,
                    CreatedAt = customer.CreatedAt
                });

                if (string.IsNullOrEmpty(customerString))
                {
                    throw new TelegramException();
                }

                await _cache.SetStringAsync($"{CacheKeys.CustomerDtoId}{customer.Id}", customerString, token);
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
            if (id <= 0 || id > long.MaxValue)
            {
                throw new TelegramException();
            }

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

                await _cache.RemoveAsync($"{CacheKeys.CustomerDtoId}{id}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{Message}", ex.Message);
                throw;
            }
        }
    }
}