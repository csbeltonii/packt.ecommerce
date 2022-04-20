using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Packt.Ecommerce.Data.Models.Models;
using Packt.Ecommerce.DataStore.Contracts;

namespace Packt.Ecommerce.DataAccess.Controllers
{
    [Route("api/orders")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _repository;

        public OrderController(IOrderRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrdersAsync(string filterCriteria = null)
        {
            IEnumerable<Order> orders;

            if (string.IsNullOrEmpty(filterCriteria))
            {
                 orders = await _repository.GetAsync(string.Empty).ConfigureAwait(false);
            }
            else
            {
                orders = await _repository.GetAsync(filterCriteria).ConfigureAwait(false);
            }

            if (orders.Any())
                return Ok(orders);

            return NoContent();

        }

        [HttpGet]
        [Route("{id}")]
        public async Task<IActionResult> GetOrderById(string id)
        {
            var result = await _repository.GetByIdAsync(id, id).ConfigureAwait(false);

            return result is null ? NoContent() : Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrderAsync([FromBody] Order order)
        {
            if (order?.Etag is null)
                return BadRequest();

            var result = await _repository.AddAsync(order, order.Id).ConfigureAwait(false);

            return CreatedAtAction(
                nameof(GetOrderById),
                new
                {
                    id = result.Resource.Id
                },
                result.Resource
            );
        }

        [HttpPut]
        public async Task<IActionResult> UpdateOrderAsync([FromBody] Order order)
        {
            if (order?.Etag is null || order.Id is null)
                return BadRequest();

            var result = await _repository.ModifyAsync(order, order.Etag, order.Id).ConfigureAwait(false);

            if (result)
                return Accepted();

            return NoContent();
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteOrderAsync(string id)
        {
            var result = await _repository.RemoveAsync(id, id).ConfigureAwait(false);

            if (result)
                return Accepted();

            return NoContent();
        }
    }
}
