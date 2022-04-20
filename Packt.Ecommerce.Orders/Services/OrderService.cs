using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Packt.Ecommerce.Cache.Interfaces;
using Packt.Ecommerce.Common.Models;
using Packt.Ecommerce.Common.Options;
using Packt.Ecommerce.DTO.Models;
using Packt.Ecommerce.Order.Contracts;

namespace Packt.Ecommerce.Order.Services
{
    public class OrderService : IOrderService
    {
        private const string ContentType = "application/json";
        private readonly IOptions<ApplicationSettings> _applicationSettings;
        private readonly IDistributedCacheService _distributedCache;
        private readonly IMapper _autoMapper;
        private readonly HttpClient _httpClient;

        public OrderService(IOptions<ApplicationSettings> applicationSettings, 
                            IDistributedCacheService distributedCache, 
                            IMapper autoMapper, IHttpClientFactory httpClientFactory)
        {
            _applicationSettings = applicationSettings;
            _distributedCache = distributedCache;
            _autoMapper = autoMapper;
            _httpClient = httpClientFactory.CreateClient();
        }

        public async Task<IEnumerable<OrderDetailsViewModel>> GetOrdersAsync(string filterCriteria = null)
        {
            var orders = await _distributedCache
                               .GetCacheAsync<IEnumerable<Data.Models.Models.Order>>($"order{filterCriteria}")
                               .ConfigureAwait(false);

            if (orders is null)
            {
                using var orderRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_applicationSettings.Value.DataStoreEndpoint}api/products?filterCritera={filterCriteria}"
                );

                var orderResponse = await _httpClient.SendAsync(orderRequest).ConfigureAwait(false);

                if (!orderResponse.IsSuccessStatusCode)
                {
                    await ThrowServiceToServiceErrors(orderResponse).ConfigureAwait(false);
                }

                orders = await orderResponse.Content
                                            .ReadFromJsonAsync<IEnumerable<Data.Models.Models.Order>>()
                                            .ConfigureAwait(false);

                if (orders.Any())
                {
                    await _distributedCache.AddOrUpdateCacheAsync($"orders{filterCriteria}", orders)
                                           .ConfigureAwait(false);
                }
            }

            var orderList = _autoMapper.Map<List<OrderDetailsViewModel>>(orders);

            return orderList;
        }

        public async Task<OrderDetailsViewModel> GetOrderByIdAsync(string orderId)
        {
            //var order = await _distributedCache.GetCacheAsync<Data.Models.Models.Order>($"order{orderId}")
            //                                   .ConfigureAwait(false);
            
            throw new NotImplementedException();
        }

        public async Task<OrderDetailsViewModel> AddOrderAsync(OrderDetailsViewModel order)
        {
            throw new System.NotImplementedException();
        }

        public async Task<HttpResponseMessage> UpdateOrderAsync(OrderDetailsViewModel order)
        {
            throw new System.NotImplementedException();
        }

        public async Task<HttpResponseMessage> DeleteOrderAsync(string orderId)
        {
            throw new System.NotImplementedException();
        }

        private async Task ThrowServiceToServiceErrors(HttpResponseMessage response)
        {
            var exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>().ConfigureAwait(false);
            throw new Exception(exceptionResponse.InnerException);
        }
    }
}
