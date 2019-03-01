using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using CustomerApi.Data;
using CustomerApi.Models;

namespace CustomerApi.Controllers
{
    [Route("api/Customers")]
    public class CustomersController : Controller
    {
        private readonly IRepository<Customer> repository;

        public CustomersController(IRepository<Customer> repos)
        {
            repository = repos;
        }

        // GET: api/customers
        [HttpGet]
        public IEnumerable<Customer> Get()
        {
            return repository.GetAll();
        }

        // GET api/customers/5
        [HttpGet("{id}", Name = "GetCustomer")]
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
        public IActionResult Post([FromBody]Customer customer)
        {
            if (customer == null)
            {
                return BadRequest();
            }

            var newCustomer = repository.Add(customer);
            return CreatedAtRoute("GetCustomer", new { id = newCustomer.customerId }, newCustomer);
        }

    }
}
