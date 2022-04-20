using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Packt.Ecommerce.Data.Models.Models;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataAccess.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductRepository _repository;

        public ProductsController(IProductRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProductAsync(string filterCriteria = null)
        {
            IEnumerable<Product> products;

            if (string.IsNullOrEmpty(filterCriteria))
            {
                products = await _repository.GetAsync(string.Empty).ConfigureAwait(false);
            }
            else
            {
                products = await _repository.GetAsync(filterCriteria).ConfigureAwait(false);
            }

            if (products.Any())
            {
                return Ok(products);
            }

            return NoContent();
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProductById(string id, [FromQuery] [Required] string name)
        {
            var result = await _repository.GetByIdAsync(id, name).ConfigureAwait(false);

            if (result is not null)
            {
                return Ok(result);
            }

            return NoContent();
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductAsync([FromBody] Product product)
        {
            if (product is null ||
                product.Etag is not null)
            {
                return BadRequest();
            }

            var result = await _repository.AddAsync(product, product.Name).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetProductById),
                new
                {
                    id = result.Resource.Id,
                    name = result.Resource.Name
                },
                result.Resource
            );
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProductAsync([FromBody] Product product){
            
            if (product is null ||
                product.Etag is null ||
                product.Id is null)
            {
                return BadRequest();
            }

            var result = await _repository.ModifyAsync(product, product.Etag, product.Name)
                                          .ConfigureAwait(false);

            if (result)
            {
                return Accepted();
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteProductAsync(string id, [FromQuery] [Required] string name)
        {
            var result = await _repository.RemoveAsync(id, name).ConfigureAwait(false);

            if (result)
            {
                return Accepted();
            }

            return NoContent();
        }
    }
}
