using System.Collections.Generic;
using System.Threading.Tasks;
using EasyNetQ;
using MessageFramework.Models;
using Microsoft.AspNetCore.Mvc;
using OrderApi.Data;

namespace OrderApi.Controllers
{
    [Route("api/Orders")]
    public class OrdersController : Controller
    {
        private readonly IBus bus;
        private readonly IRepository<Order> repository;

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
            if (item == null) return NotFound();
            return new ObjectResult(item);
        }

        // GET api/products/5
        [HttpGet("{customerId}", Name = "GetOrdersByCustomer")]
        public IEnumerable<Order> GetOrdersByCustomer(int customerId)
        {
            var orders = repository.GetAll();

            var orderByCustomer = new Stack<Order>();

            foreach (var order in orders)
                if (order.CustomerId == customerId)
                    orderByCustomer.Push(order);

            if (orderByCustomer.Count == 0)
                return null;

            return orderByCustomer;
        }

        // POST api/orders
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Order order, Customer customer)
        {
            if (order == null) return BadRequest();

            if (customer == null) return BadRequest();


            var customerMessage = await bus.RequestAsync<Customer, CustomerResponse>(customer);
            if (customerMessage.customer == null) return BadRequest("Customer does not exist");

            customer = customerMessage.customer;
            if (!customerMessage.IsSuccessful) return BadRequest("Customer do not exist");
            if (customer.creditStanding != Credit.GOOD) return BadRequest("Bad credit");

            var orders = GetOrdersByCustomer(customer.customerId);
            var paidOrders = true;
            foreach (var item in orders)
                if (item.PaymentStatus == PaymentStatus.UNPAID)
                    paidOrders = false;
            if (!paidOrders) return BadRequest("You have an unpaid order");

            foreach (var orderProduct in order.Products)
            {
                var orderedProduct = await bus.RequestAsync<ProductRequest, Product>(new ProductRequest
                    {ProductId = orderProduct.Product.Id});

                if (orderProduct.Quantity > orderedProduct.ItemsInStock - orderedProduct.ItemsReserved)
                    return BadRequest($"There are not enough in stock of {orderProduct.Product.Name}");

                orderedProduct.ItemsReserved += orderProduct.Quantity;
                var updateProductRequest = await bus.RequestAsync<Product, ProductResponse>(orderedProduct);
                if (!updateProductRequest.IsSuccessful)
                    return BadRequest($"Error while reserving {orderProduct.Quantity} of {orderProduct.Product.Name}");
            }

            var newOrder = repository.Add(order);
            return CreatedAtRoute("GetOrder", new {id = newOrder.Id}, newOrder);
        }
    }
}