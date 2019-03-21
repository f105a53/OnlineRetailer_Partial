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

        // GET api/products/5
        [HttpGet("{customerId}", Name = "GetOrdersByCustomer")]
        public IEnumerable<Order> GetOrdersByCustomer(int customerId)
        {
            var orders = repository.GetAll();

            Stack<Order> orderByCustomer = new Stack<Order>();

            foreach (Order order in orders)
            {
                if (order.CustomerId == customerId)
                {
                    orderByCustomer.Push(order);
                }
            }

            if (orderByCustomer.Count == 0)
                return null;

            return orderByCustomer;
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Order order, Customer customer)
        {
            if (order == null)
            {
                return BadRequest();
            }

            if (customer == null)
            {
                return BadRequest();
            }


            var customerMessage = await bus.RequestAsync<Customer, CustomerResponse>(customer);

            if (customerMessage.customer != null)
            {
                customer = customerMessage.customer;
                if (customerMessage.IsSuccessful)
                {
                    if (customer.creditStanding == Credit.GOOD)
                    {
                        IEnumerable<Order> orders = GetOrdersByCustomer(customer.customerId);

                        bool paidOrders = true;

                        foreach (Order item in orders)
                        {
                            if (item.PaymentStatus == PaymentStatus.UNPAID)
                            {
                                paidOrders = false;
                            }
                        }

                        if (paidOrders)
                        {
                            foreach (OrderProduct orderProduct in order.Products)
                            {
                                var orderedProduct = await bus.RequestAsync<ProductRequest, Product>(new ProductRequest() { ProductId = orderProduct.Product.Id });

                                if (orderProduct.Quantity <= orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                                {
                                    orderedProduct.ItemsReserved += orderProduct.Quantity;

                                    var updateProductRequest = await bus.RequestAsync<Product, ProductResponse>(orderedProduct);

                                    if (!updateProductRequest.IsSuccessful)
                                        return CreatedAtRoute(new { message = string.Format("There are not enough in stock of {0}", orderProduct.Product.Name) }, null);
                                }

                            }

                            var newOrder = repository.Add(order);
                            return CreatedAtRoute("GetOrder", new { id = newOrder.Id }, newOrder);


                        }
                        else
                            return CreatedAtRoute(new { message = "You have an unpaid order" }, null);

                    }
                    else
                        return CreatedAtRoute(new { message = "Bad credit" }, null);

                }
                else
                    return CreatedAtRoute(new { message = "Customer do not exist" }, null);

            }

            return NoContent();
        }

    }
}
