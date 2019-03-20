using EasyNetQ;
using MessageFramework.Models;
using ProductApi.Data;

namespace ProductApi.Services
{
    internal class MessageHandler
    {
        public MessageHandler(IBus bus, IRepository<Product> repository)
        {
            bus.Respond<ProductRequest, Product>(req => repository.Get(req.ProductId));
            bus.Respond<Product, ProductResponse>(req =>
            {
                try
                {
                    repository.Edit(req);
                }
                catch
                {
                    return new ProductResponse {IsSuccessful = false};
                }

                return new ProductResponse {IsSuccessful = true};
            });
        }
    }
}