using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using TennisScores.Infrastructure.Data;

namespace TennisScores.Tests.Integration.Services;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remplacer le DbContext existant par une version InMemory
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<TennisDbContext>));

            if (descriptor != null)
                services.Remove(descriptor);

            services.AddDbContext<TennisDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Optional : Seed base ici (ou dans test Setup)
        });
    }
}
