using System;
using System.Collections.Generic;

namespace MessageFramework.Models
{
    public enum OrderStatus
    {
        COMPLETED,
        CANCELLED,
        SHIPPED
    }

    public enum PaymentStatus
    {
        PAID,
        UNPAID
    }
    public class Order
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public List<OrderProduct> Products { get; set; }
        public int CustomerId { get; set; }
        public OrderStatus Status { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}
