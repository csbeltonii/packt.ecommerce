using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using Packt.Ecommerce.Data.Models.Models;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataStore
{
    public class  InvoiceRepository : BaseRepository<Invoice>, IInvoiceRepository
    {
        private readonly IOptions<DatabaseSettingsOptions> databaseSettings;

        public InvoiceRepository(CosmosClient cosmosClient, IOptions<DatabaseSettingsOptions> databaseSettingsOptions)
            : base(
                cosmosClient,
                databaseSettingsOptions.Value.DataBaseName,
                "Invoice"
            )
        {
            databaseSettings = databaseSettingsOptions;
        }
    }
}
