using BankService.DB;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BankService.IntegrationTests
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<BankingContext>
                    (options => options.UseSqlServer($"Server=(localdb)\\MSSQLLocalDB;Database=BankServiceIntegrationTest;Trusted_Connection=True;ConnectRetryCount=0"));
            });
        }
    }
}