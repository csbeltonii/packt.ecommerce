using System;
using System.Net;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Packt.Ecommerce.Cache;
using Packt.Ecommerce.Cache.Interfaces;
using Packt.Ecommerce.Common.Options;
using Packt.Ecommerce.Product.Contracts;
using Packt.Ecommerce.Product.Services;
using Polly;
using Polly.Extensions.Http;

namespace Packt.Ecommerce.Product
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddAutoMapper(typeof(AutoMapperProfile));

            services.Configure<ApplicationSettings>(Configuration.GetSection("ApplicationSettings"));

            services.AddHttpClient<IProductService, ProductsService>()
                    .SetHandlerLifetime(TimeSpan.FromMinutes(5))
                    .AddPolicyHandler(RetryPolicy())
                    .AddPolicyHandler(CircuitBreakerPolicy());

            services.AddSingleton<IEntitySerializer, EntitySerializer>();
            services.AddSingleton<IDistributedCacheService, DistributedCacheService>();
            services.AddScoped<IProductService, ProductsService>();

            if (Configuration.GetValue<bool>("ApplicationSettings:UseRedis"))
            {
                services.AddStackExchangeRedisCache(
                    option =>
                    {
                        option.Configuration = Configuration.GetConnectionString("Redis");
                    }
                );
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Packt.Ecommerce.Product", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Packt.Ecommerce.Product v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError().CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }

        private static IAsyncPolicy<HttpResponseMessage> RetryPolicy()
        {
            var random = new Random();

            var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
                                                  .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
                                                  .WaitAndRetryAsync(
                                                      5,
                                                      retry => TimeSpan.FromSeconds(Math.Pow(2, retry)) +
                                                               TimeSpan.FromMilliseconds(random.Next(0, 100))
                                                  );

            return retryPolicy;
        }
    }
}
