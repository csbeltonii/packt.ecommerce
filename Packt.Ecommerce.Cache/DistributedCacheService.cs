using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Packt.Ecommerce.Cache.Interfaces;

namespace Packt.Ecommerce.Cache
{
    public class DistributedCacheService : IDistributedCacheService
    {
        private const long DefaultCacheAbsoluteExpirationMinutes = 60;
        private readonly IEntitySerializer entitySerializer;
        private readonly IDistributedCache distributedCache;
        private readonly DistributedCacheEntryOptions distributedCacheEntryOptions;
        private readonly TimeSpan defaultCacheEntryAbsoluteExpirationTime;

        public DistributedCacheService(IEntitySerializer entitySerializer, 
                                       IDistributedCache distributedCache)
        {
            this.entitySerializer = entitySerializer;
            this.distributedCache = distributedCache;
            this.distributedCacheEntryOptions = new DistributedCacheEntryOptions();
            this.defaultCacheEntryAbsoluteExpirationTime = TimeSpan.FromMinutes(DefaultCacheAbsoluteExpirationMinutes);
        }

        public async Task AddOrUpdateCacheAsync<T>(string cacheEntityKey,
                                                   T cacheEntity,
                                                   TimeSpan? absoluteExpiration = default,
                                                   CancellationToken cancellationToken = default)
        {
            var absoluteExpiryTime = absoluteExpiration == null
                ? defaultCacheEntryAbsoluteExpirationTime
                : TimeSpan.FromSeconds(absoluteExpiration.Value.TotalSeconds);

            var byteValue = await entitySerializer
                            .SerializeEntityAsync<T>(cacheEntity)
                            .ConfigureAwait(false);

            await distributedCache.SetAsync(
                                      cacheEntityKey,
                                      byteValue,
                                      distributedCacheEntryOptions.SetAbsoluteExpiration(absoluteExpiryTime),
                                      cancellationToken
                                  )
                                  .ConfigureAwait(false);
        }

        public async Task AddOrUpdateCacheStringAsync(string cacheEntityKey,
                                                      string cacheEntity,
                                                      TimeSpan? absoluteExpiration = default,
                                                      CancellationToken cancellationToken = default)
        {
            var absoluteExpiryTime = absoluteExpiration == null
                ? defaultCacheEntryAbsoluteExpirationTime
                : TimeSpan.FromSeconds(absoluteExpiration.Value.TotalSeconds);

            await distributedCache.SetStringAsync(
                                      cacheEntityKey,
                                      cacheEntity,
                                      distributedCacheEntryOptions.SetAbsoluteExpiration(absoluteExpiryTime), 
                                      cancellationToken
                                  )
                                  .ConfigureAwait(false);
        }

        public async Task<T> GetCacheAsync<T>(string cacheEntityKey, CancellationToken cancellationToken = default)
        {
            var obj = await distributedCache.GetAsync(cacheEntityKey, cancellationToken).ConfigureAwait(false);

            return (obj != null
                ? await entitySerializer.DeserializeEntityAsync<T>(obj).ConfigureAwait(false)
                : default)!;
        }

        public async Task<string> GetCacheStringAsync(string cacheEntityKey, CancellationToken cancellationToken = default)
        {
            return await distributedCache
                         .GetStringAsync(cacheEntityKey, cancellationToken)
                         .ConfigureAwait(false);
        }

        public async Task RefreshCacheAsync(string cacheEntityKey, CancellationToken cancellationToken = default)
        {
            await distributedCache
                  .RefreshAsync(cacheEntityKey, cancellationToken)
                  .ConfigureAwait(false);
        }

        public async Task RemoveCacheAsync(string cacheEntityKey, CancellationToken cancellationToken = default)
        {
            await distributedCache
                  .RemoveAsync(cacheEntityKey, cancellationToken)
                  .ConfigureAwait(false);
        }
    }
}
