using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataStore
{
    public class BaseRepository<TEntity> : IBaseRepository<TEntity>
        where TEntity : class
    {
        private readonly Container container;

        public BaseRepository(CosmosClient cosmosClient, string databaseName, string containerName)
        {
            if (cosmosClient == null)
            {
                throw new Exception("Cosmos Client is null");
            }

            container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public async Task<IEnumerable<TEntity>> GetAsync(string filterCriteria)
        {
            if (string.IsNullOrWhiteSpace(filterCriteria))
            {
                filterCriteria = "select * from e";
            }
            else
            {
                filterCriteria = $"select * from e where {filterCriteria}";
            }

            var iterator = container.GetItemQueryIterator<TEntity>(new QueryDefinition(filterCriteria));
            var results = new List<TEntity>();

            while (iterator.HasMoreResults)
            {
                var result = await iterator.ReadNextAsync().ConfigureAwait(false);

                results.AddRange(result);
            }

            return results;
        }

        public async Task<TEntity> GetByIdAsync(string id, string partitionKey)
        {
            try
            {
                var response = await container.ReadItemAsync<TEntity>(id, new PartitionKey(partitionKey))
                                              .ConfigureAwait(false);

                return response;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<ItemResponse<TEntity>> AddAsync(TEntity entity, string partitionKey)
        {
            try
            {
                var response = await container.CreateItemAsync<TEntity>(entity, new PartitionKey(partitionKey))
                                              .ConfigureAwait(false);
                
                return response;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            return null;
        }

        public async Task<bool> ModifyAsync(TEntity entity, string etag, string partitionKey)
        {
            try
            {
                var response = await container.UpsertItemAsync<TEntity>(
                                                  entity,
                                                  new PartitionKey(partitionKey),
                                                  new ItemRequestOptions
                                                  {
                                                      IfMatchEtag = etag
                                                  }
                                              )
                                              .ConfigureAwait(false);

                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode != HttpStatusCode.NotFound ||
                                             ex.StatusCode == HttpStatusCode.PreconditionFailed)
            {
                return false;
            }
        }

        public async Task<bool> RemoveAsync(string id, string partitionKey)
        {
            try
            {
                var response = await container.DeleteItemAsync<TEntity>(id, new PartitionKey(partitionKey)).ConfigureAwait(false);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return false;
            }
        }
    }
}
