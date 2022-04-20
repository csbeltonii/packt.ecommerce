using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataStore
{
    public class UserRepository : BaseRepository<User>, IUserRepository
    {
        private readonly IOptions<DatabaseSettingsOptions> databaseSettings;

        public UserRepository(CosmosClient cosmosClient, IOptions<DatabaseSettingsOptions> databaseSettingsOptions) : base(
            cosmosClient,
            databaseSettingsOptions.Value.DataBaseName,
            "Users"
        )
        {
            databaseSettings = databaseSettingsOptions;
        }
    }
}
