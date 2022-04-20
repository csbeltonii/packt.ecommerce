using System.Configuration;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Packt.Ecommerce.DataStore;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataAccess
{
    public static class RepositoryExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            return services;
        }

        public static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<DatabaseSettingsOptions>(configuration.GetSection("CosmosDB"));
            var accountEndPoint = configuration.GetValue<string>("CosmosDB:AccountEndPoint");
            var authKey = configuration.GetValue<string>("CosmosDB:AuthKey");
            services.AddSingleton(s => new CosmosClient(accountEndPoint, authKey));

            return services;
        }
    }
}
