using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using OMarket.Domain.DTOs;
using OMarket.Domain.Entities;
using OMarket.Domain.Exceptions.Telegram;
using OMarket.Domain.Interfaces.Application.Services.Cache;
using OMarket.Domain.Interfaces.Infrastructure.Repositories;
using OMarket.Helpers.Utilities;
using OMarket.Infrastructure.Data.Contexts.ApplicationContext;

namespace OMarket.Infrastructure.Repositories
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IDbContextFactory<AppDBContext> _contextFactory;

        private readonly ILogger<ReviewRepository> _logger;

        private readonly ICacheService _cache;

        private readonly int _pageSize = 1;

        public ReviewRepository(
                IDbContextFactory<AppDBContext> contextFactory,
                ILogger<ReviewRepository> logger,
                ICacheService cache
            )
        {
            _contextFactory = contextFactory;
            _logger = logger;
            _cache = cache;
        }

        public async Task AddNewReviewAsync(long id, Guid storeId, string text, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (storeId == Guid.Empty || text.Length >= 256 || string.IsNullOrEmpty(text))
            {
                throw new TelegramException();
            }

            try
            {
                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                bool isNewReviews = await context.Reviews
                    .Where(review => review.CustomerId == id && review.Text == text)
                    .AnyAsync(token);

                if (isNewReviews)
                {
                    return;
                }

                await context.Reviews.AddAsync(new Review()
                {
                    Text = text,
                    CustomerId = id,
                    StoreId = storeId,
                }, cancellationToken: token);

                int maxPages = await context.Reviews
                    .AsNoTracking()
                    .Where(review => review.StoreId == storeId)
                    .CountAsync(token);

                await context.SaveChangesAsync(token);

                for (int page = 1; page <= maxPages; page++)
                {
                    await _cache.RemoveCacheAsync($"{CacheKeys.ReviewId}{storeId}-{_pageSize}-{page}");
                }
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

        public async Task<ReviewWithDbInfoDto?> GetReviewWithPaginationAsync(int pageNumber, Guid storeId, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            if (pageNumber <= 0 || storeId == Guid.Empty)
            {
                throw new TelegramException();
            }

            ReviewWithDbInfoDto? review;
            string cacheKey = $"{CacheKeys.ReviewId}{storeId}-{_pageSize}-{pageNumber}";

            review = await _cache.GetCacheAsync<ReviewWithDbInfoDto>(cacheKey);

            if (review is not null)
                return review;

            try
            {
                await using AppDBContext context = await _contextFactory.CreateDbContextAsync(token);

                var reviews = await context.Reviews
                   .AsNoTracking()
                   .Where(review => review.StoreId == storeId)
                   .OrderByDescending(review => review.CreatedAt)
                   .Skip((pageNumber - 1) * _pageSize)
                   .Take(_pageSize)
                   .Select(review => new ReviewDto()
                   {
                       Id = review.Id,
                       Text = review.Text,
                       CustomerId = review.CustomerId,
                       StoreId = review.StoreId,
                       CreatedAt = review.CreatedAt,
                   })
                   .ToListAsync(token);

                int maxPageNumber = await context.Reviews
                    .AsNoTracking()
                    .Where(review => review.StoreId == storeId)
                    .CountAsync(token);

                review = reviews
                    .Select(review => new ReviewWithDbInfoDto()
                    {
                        Id = review.Id,
                        Review = review,
                        PageNumber = pageNumber,
                        MaxNumber = maxPageNumber
                    })
                    .ToArray()
                    .ElementAtOrDefault(0);

                if (review is null)
                {
                    return null;
                }

                await _cache.SetCacheAsync(cacheKey, review);

                return review;
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
    }
}