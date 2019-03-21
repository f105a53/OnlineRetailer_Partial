using CustomerApi.Data;
using EasyNetQ;
using MessageFramework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CustomerApi.Services
{
    public class MessageHandler
    {
        public MessageHandler(IBus bus, IRepository<Customer> repository)
        {
            bus.Respond<Customer, CustomerResponse>(req => 
            {
                Customer customer = repository.Get(req.customerId);

                if (customer != null)
                {
                    return new CustomerResponse() { IsSuccessful = true, customer = customer };
                }

                return new CustomerResponse() { IsSuccessful = true, customer = null};
            });
        }
    }
}
