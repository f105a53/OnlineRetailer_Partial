using System;
using System.Collections.Generic;
using System.Text;

namespace MessageFramework.Models
{
    public class CustomerResponse
    {
        public bool IsSuccessful { get; set; }
        public Customer customer { get; set; }
    }
}
