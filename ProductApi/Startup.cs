using EasyNetQ;
using MessageFramework.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ProductApi.Data;
using Swashbuckle.AspNetCore.Swagger;

namespace ProductApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // In-memory database:
            services.AddDbContext<ProductApiContext>(opt =>
                opt.UseInMemoryDatabase("ProductsDb").EnableSensitiveDataLogging());

            // Register repositories for dependency injection
            services.AddScoped<IRepository<Product>, ProductRepository>();

            // Register database initializer for dependency injection
            services.AddTransient<IDbInitializer, DbInitializer>();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            services.AddSingleton(RabbitHutch.CreateBus(
                "amqp://styjxehb:SfZDHmtVwzdfYxFSHynoLXyeRltIC320@bullfrog.rmq.cloudamqp.com/styjxehb"));

            services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new Info {Title = "ProductAPI", Version = "v1"}); });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Initialize the database
            using (var scope = app.ApplicationServices.CreateScope())
            {
                // Initialize the database
                var services = scope.ServiceProvider;
                var dbContext = services.GetService<ProductApiContext>();
                var dbInitializer = services.GetService<IDbInitializer>();
                dbInitializer.Initialize(dbContext);
            }

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }


            app.UseHttpsRedirection();
            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1"); });

            var bus = app.ApplicationServices.GetService<IBus>();

            bus.Respond<ProductRequest, Product>(req =>
            {
                using (var serviceScope = app.ApplicationServices.CreateScope())
                {
                    var repository = serviceScope.ServiceProvider.GetService<IRepository<Product>>();
                    return repository.Get(req.ProductId);
                }
            });
            bus.Respond<Product, ProductResponse>(req =>
            {
                try
                {
                    using (var serviceScope = app.ApplicationServices.CreateScope())
                    {
                        var repository = serviceScope.ServiceProvider.GetService<IRepository<Product>>();
                        repository.Edit(req);
                    }
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