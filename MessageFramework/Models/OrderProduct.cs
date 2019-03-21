using System;
using System.Collections.Generic;
using System.Text;

namespace MessageFramework.Models
{
    public class OrderProduct
    {
        public int Id { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
