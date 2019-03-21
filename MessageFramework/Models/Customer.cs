using System;
namespace MessageFramework.Models
{
    public enum Credit
    {
        BAD,
        GOOD
    }

    public class Customer
    {
        public int customerId { get; set; }
        public string name { get; set; }
        public string email { get; set; }
        public int phone { get; set; }
        public string billingAddress { get; set; }
        public string shippingAddress { get; set; }
        public Credit creditStanding { get; set; }
    }
}
