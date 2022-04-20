using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Packt.Ecommerce.DTO.Models;
using Packt.Ecommerce.Product.Contracts;

namespace Packt.Ecommerce.Product.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetProductById(string id, 
                                                        [FromQuery] [Required] string name)
        {
            var product = await _productService.GetProductByIdAsync(id, name).ConfigureAwait(false);

            if (product != null)
            {
                return Ok(product);
            }
            else
            {
                return NoContent();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddProductAsync(ProductDetailsViewModel product)
        {
            if (product is null ||
                product.Etag is not null)
            {
                return BadRequest();
            }

            var result = await _productService.AddProductAsync(product).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetProductById),
                new
                {
                    id = result.Id,
                    name = result.Name
                },
                result
            );
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts([FromQuery]string filterCriteria = null)
        {
            var products = await _productService.GetProductsAsync(filterCriteria).ConfigureAwait(false);

            if (products.Any())
            {
                return Ok(products);
            }
            else
            {
                return NoContent();
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct(ProductDetailsViewModel product)
        {
            if (product is null ||
                product.Etag is null ||
                product.Id is null)
            {
                return BadRequest();
            } 

            var result = await _productService.UpdateProductAsync(product).ConfigureAwait(false);

            if (result.StatusCode == HttpStatusCode.Accepted)
            {
                return Accepted();
            }

            return NoContent();
        }

        [HttpDelete]
        [Route("{id}")]
        public async Task<IActionResult> DeleteProduct(string id, [FromQuery] [Required] string name)
        {
            var result = await _productService.DeleteProductAsync(id, name).ConfigureAwait(false);

            if (result.StatusCode == HttpStatusCode.Accepted)
            {
                return Accepted();
            }

            return NoContent();
        }
    }
}
