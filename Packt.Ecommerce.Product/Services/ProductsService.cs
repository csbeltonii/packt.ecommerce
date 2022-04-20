using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Options;
using Packt.Ecommerce.Cache.Interfaces;
using Packt.Ecommerce.Common.Models;
using Packt.Ecommerce.Common.Options;
using Packt.Ecommerce.Common.Validator;
using Packt.Ecommerce.DTO.Models;
using Packt.Ecommerce.Product.Contracts;

namespace Packt.Ecommerce.Product.Services
{
    public class ProductsService : IProductService
    {
        private const string ContentType = "application/json";
        private readonly IOptions<ApplicationSettings> _applicationSettings;
        private readonly HttpClient _httpClient;
        private readonly IMapper _autoMapper;
        private readonly IDistributedCacheService _cacheService;

        public ProductsService(IOptions<ApplicationSettings> applicationSettings, IHttpClientFactory httpClientFactory, IMapper autoMapper, IDistributedCacheService cacheService)
        {
            NotNullValidator.ThrowIfNull(applicationSettings, nameof(applicationSettings));

            _applicationSettings = applicationSettings;
            _httpClient = httpClientFactory.CreateClient();
            _autoMapper = autoMapper;
            _cacheService = cacheService;
        }

        public async Task<IEnumerable<ProductListViewModel>> GetProductsAsync(string filterCriteria = null)
        {
            var products = await _cacheService
                           .GetCacheAsync<IEnumerable<Data.Models.Models.Product>>($"products{filterCriteria}")
                           .ConfigureAwait(false);

            if (products == null)
            {
                using var productRequest = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"{_applicationSettings.Value.DataStoreEndpoint}api/products?filterCriteria={filterCriteria}"
                );

                var productResponse = await _httpClient.SendAsync(productRequest).ConfigureAwait(false);

                if (!productResponse.IsSuccessStatusCode)
                {
                    await ThrowServiceToServiceErrors(productResponse).ConfigureAwait(false);
                }

                products = await productResponse.Content.ReadFromJsonAsync<IEnumerable<Data.Models.Models.Product>>()
                                                .ConfigureAwait(false);

                if (products.Any())
                {
                    await _cacheService
                          .AddOrUpdateCacheAsync<IEnumerable<Data.Models.Models.Product>>(
                              $"products{filterCriteria}",
                              products
                          )
                          .ConfigureAwait(false);
                }
            }

            var productList = _autoMapper.Map<List<ProductListViewModel>>(products);

            return productList;
        }

        public async Task<ProductDetailsViewModel> GetProductByIdAsync(string productId, string productName)
        {
            using var productRequest = new HttpRequestMessage(
                HttpMethod.Get,
                $"{_applicationSettings.Value.DataStoreEndpoint}api/products/{productId}?name={productName}"
            );

            var productResponse = await _httpClient.SendAsync(productRequest).ConfigureAwait(false);

            if (!productResponse.IsSuccessStatusCode)
            {
                await ThrowServiceToServiceErrors(productResponse).ConfigureAwait(false);
            }

            if (productResponse.StatusCode != HttpStatusCode.NoContent)
            {
                var productDAO = await productResponse.Content.ReadFromJsonAsync<Data.Models.Models.Product>()
                                                      .ConfigureAwait(false);

                var product = _autoMapper.Map<ProductDetailsViewModel>(productDAO);

                return product;
            }
            else
            {
                return null;
            }
        }

        public async Task<ProductDetailsViewModel> AddProductAsync(ProductDetailsViewModel product)
        {
            NotNullValidator.ThrowIfNull(product, nameof(product));

            product.CreatedDate = DateTime.UtcNow;

            using var productRequest = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, ContentType);
            
            var productResponse = await _httpClient.PostAsync(
                                                       new Uri(
                                                           $"{_applicationSettings.Value.DataStoreEndpoint}api/products"
                                                       ),
                                                       productRequest
                                                   )
                                                   .ConfigureAwait(false);

            if (!productResponse.IsSuccessStatusCode)
            {
                await ThrowServiceToServiceErrors(productResponse).ConfigureAwait(false);
            }

            var createdProductDAO = await productResponse.Content
                                                         .ReadFromJsonAsync<Data.Models.Models.Product>()
                                                         .ConfigureAwait(false);

            await _cacheService.RemoveCacheAsync("products").ConfigureAwait(false);

            var createdProduct = _autoMapper.Map<ProductDetailsViewModel>(createdProductDAO);

            return createdProduct;
        }

        public async Task<HttpResponseMessage> UpdateProductAsync(ProductDetailsViewModel product)
        {
            using var productRequest = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, ContentType);

            var productResponse = await _httpClient.PutAsync(
                                                 new Uri($"{_applicationSettings.Value.DataStoreEndpoint}api/products"),
                                                 productRequest
                                             )
                                             .ConfigureAwait(false);

            if (!productResponse.IsSuccessStatusCode)
            {
                await ThrowServiceToServiceErrors(productResponse).ConfigureAwait(false);
            }

            await _cacheService.RemoveCacheAsync("products").ConfigureAwait(false);

            return productResponse;
        }

        public async Task<HttpResponseMessage> DeleteProductAsync(string productId, string productName)
        {
            var productResponse = await _httpClient.DeleteAsync(
                new Uri($"{_applicationSettings.Value.DataStoreEndpoint}api/products/{productId}?name={productName}")
            );

            if (!productResponse.IsSuccessStatusCode)
            {
                await ThrowServiceToServiceErrors(productResponse).ConfigureAwait(false);
            }

            await _cacheService.RemoveCacheAsync("products").ConfigureAwait(false);

            return productResponse;
        }

        private async Task ThrowServiceToServiceErrors(HttpResponseMessage response)
        {
            var exceptionResponse = await response.Content.ReadFromJsonAsync<ExceptionResponse>().ConfigureAwait(false);
            throw new Exception(exceptionResponse.InnerException);
        }
    }
}
