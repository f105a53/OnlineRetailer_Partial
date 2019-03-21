using System.Collections.Generic;
using System.Linq;
using System;
using MessageFramework.Models;

namespace OrderApi.Data
{
    public class DbInitializer : IDbInitializer
    {
        // This method will create and seed the database.
        public void Initialize(OrderApiContext context)
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Look for any Products
            if (context.Orders.Any())
            {
                return;   // DB has been seeded
            }

            List<OrderProduct> orderProducts1 = new List<OrderProduct>()
            {
                new OrderProduct { Product = new Product() { Name = "Hammer", Price = 100}, Quantity = 1 }
            };

            List<Order> orders = new List<Order>
            {
                new Order() { CustomerId = 1, Products = orderProducts1, PaymentStatus = PaymentStatus.PAID, Status = OrderStatus.SHIPPED}
            };

            context.Orders.AddRange(orders);
            context.SaveChanges();
        }
    }
}
