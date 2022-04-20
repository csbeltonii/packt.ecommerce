using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Packt.Ecommerce.Data.Models.Models;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataStore
{
    public class ProductRepository : BaseRepository<Product>, IProductRepository
    {
        private readonly IOptions<DatabaseSettingsOptions> databaseSettings;

        public ProductRepository(CosmosClient cosmosClient, IOptions<DatabaseSettingsOptions> databaseSettingsOption) : base(
            cosmosClient,
            databaseSettingsOption?.Value.DataBaseName,
            "Products"
        )
        {
            databaseSettings = databaseSettingsOption;
        }
    }
}
