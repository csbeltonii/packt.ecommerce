using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Packt.Ecommerce.Cache.Interfaces;
using System.Text.Json;
using Packt.Ecommerce.Common.Validator;

namespace Packt.Ecommerce.Cache
{
    public class EntitySerializer : IEntitySerializer
    {
        public async Task<byte[]> SerializeEntityAsync<T>(T entity, CancellationToken cancellationToken = default)
        {
            await using var memoryStream = new MemoryStream();

            await JsonSerializer
                  .SerializeAsync<T>(memoryStream, entity, cancellationToken: cancellationToken)
                  .ConfigureAwait(false);

            memoryStream.Seek(0, SeekOrigin.Begin);

            return memoryStream.ToArray();
        }

        public async Task<T> DeserializeEntityAsync<T>(byte[] entity, CancellationToken cancellationToken = default)
        {
            NotNullValidator.ThrowIfNull(entity, nameof(entity));

            await using var memoryStream = new MemoryStream(entity);

            var value = await JsonSerializer
                              .DeserializeAsync<T>(memoryStream, cancellationToken: cancellationToken)
                              .ConfigureAwait(false);

            return value;
        }
    }
}
