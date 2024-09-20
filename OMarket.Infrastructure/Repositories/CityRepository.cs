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

namespace OMarket.Infrastructure.Repositories
{
    public class CityRepository : ICityRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<CustomersRepository> _logger;

        private readonly IDistributedCache _cache;

        private readonly IMapper _mapper;

        public CityRepository(
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

        public async Task<List<CityDto>> GetAllCitiesAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            List<CityDto> cities;

            string? citiesString = await _cache.GetStringAsync(CacheKeys.ListCitiesDtoId, token);

            if (string.IsNullOrEmpty(citiesString))
            {
                token.ThrowIfCancellationRequested();

                try
                {
                    await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                    List<City> citiesEntities = await context.Cities
                        .AsNoTracking()
                        .ToListAsync(token);

                    cities = citiesEntities.Select(city => new CityDto
                    {
                        Id = city.Id,
                        CityName = city.CityName
                    }).ToList();

                    citiesString = JsonSerializer.Serialize<List<CityDto>>(cities);

                    if (string.IsNullOrEmpty(citiesString))
                    {
                        throw new TelegramException();
                    }

                    await _cache.SetStringAsync(CacheKeys.ListCitiesDtoId, citiesString, token);
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

                cities = JsonSerializer.Deserialize<List<CityDto>>(citiesString)
                    ?? throw new TelegramException();
            }

            if (cities is null)
            {
                throw new TelegramException();
            }

            return cities;
        }
    }
}