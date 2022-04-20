using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using Packt.Ecommerce.Common.Contants;
using Packt.Ecommerce.Common.Models;
using Packt.Ecommerce.Common.Options;

#nullable disable

namespace Packt.Ecommerce.Common.Middlewares
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _requestDelegate;
        private readonly ILogger _logger;
        private readonly bool _includeExceptionDetailsInResponse;

        public ErrorHandlingMiddleware(RequestDelegate requestDelegate,
                                       ILogger<ErrorHandlingMiddleware> logger,
                                       IOptions<ApplicationSettings> applicationSettings)

        {
            _requestDelegate = requestDelegate;
            _logger = logger;
            _includeExceptionDetailsInResponse = applicationSettings.Value.IncludeExceptionStackInResponse;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                if (_requestDelegate != null)
                {
                    // invoke the next middleware
                    await _requestDelegate.Invoke(context)
                                    .ConfigureAwait(false);
                }
            }
            catch (Exception innerException)
            {
                _logger.LogCritical(1001, innerException, "Exception captured in error handling middleware.");

                var currentException = new ExceptionResponse()
                {
                    ErrorMessage = Constants.ErrorMiddlewareLog,
                    CorrelationIdentifier = System.Diagnostics.Activity.Current?.RootId
                };
                
                if (_includeExceptionDetailsInResponse)
                {
                    currentException.InnerException = $"{innerException.Message} {innerException.StackTrace}";
                }

                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                
                await context.Response.WriteAsync(JsonSerializer.Serialize(innerException)).ConfigureAwait(false);
            }
        }
    }
}
