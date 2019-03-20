using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using MessageFramework.Models;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;
using RestSharp;

namespace OrderApi.Controllers
{
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly IRepository<Order> repository;
        private readonly IBus bus;

        public OrdersController(IRepository<Order> repos, IBus bus)
        {
            this.bus = bus;
            repository = repos;
        }

        // GET: api/orders
        [HttpGet]
        public IEnumerable<Order> Get()
        {
            return repository.GetAll();
        }

        // GET api/products/5
        [HttpGet("{id}", Name = "GetOrder")]
        public IActionResult Get(int id)
        {
            var item = repository.Get(id);
            if (item == null)
            {
                return NotFound();
            }
            return new ObjectResult(item);
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Order order)
        {
            if (order == null)
            {
                return BadRequest();
            }

            var orderedProduct = await bus.RequestAsync<ProductRequest, Product>(new ProductRequest() { ProductId = order.ProductId });

            if (order.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
            {
                orderedProduct.ItemsReserved += order.Quantity;

                var updateProductRequest = await bus.RequestAsync<Product, ProductResponse>(orderedProduct);

                if (updateProductRequest.IsSuccessful) {
                    var newOrder = repository.Add(order);
                    return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
                }
            }


            //if (order.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
            //{
            //    // reduce the number of items in stock for the ordered product,
            //    // and create a new order.
            //    orderedProduct.ItemsReserved += order.Quantity;
            //    var updateRequest = new RestRequest(orderedProduct.Id.ToString(), Method.PUT);
            //    updateRequest.AddJsonBody(orderedProduct);
            //    var updateResponse = c.Execute(updateRequest);

            //    if (updateResponse.IsSuccessful)
            //    {
            //        var newOrder = repository.Add(order);
            //        return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);
            //    }
            //}

            // If the order could not be created, "return no content".
            return NoContent();
        }

    }
}
