using System.Collections.Generic;
using System.Linq;
using System;
using MessageFramework.Models;

namespace CustomerApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(CustomerApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Customers
            if (context.Customers.Any())
            {
                return;   // DB has been seeded
            }

            List<Customer> customers = new List<Customer>
            {
                new Customer { customerId = 1, name = "John Hitler", email = "hitler@nazi.de", phone = 88888888, billingAddress = "berlin strasse 2, berlin", shippingAddress = "berlin strasse 2, berlin", creditStanding = "AAA" }
            };

            context.Customers.AddRange(customers);
            context.SaveChanges();
        }
    }
}
