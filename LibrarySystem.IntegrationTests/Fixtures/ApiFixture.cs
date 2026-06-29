using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.IntegrationTests.Fixtures;

public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IServiceScope _serviceScope = null!;
    private const string IntegrationTestInMemoryDatabaseName = "IntegrationTestDb"; 
    public LibrarySystemContext LibrarySystemContext { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<LibrarySystemContext>(options =>
            {
                options.UseInMemoryDatabase(IntegrationTestInMemoryDatabaseName);
            });
        });
    }
    public async Task InitializeAsync()
    {
        _serviceScope = Services.CreateScope();
        LibrarySystemContext = _serviceScope.ServiceProvider
            .GetRequiredService<LibrarySystemContext>();
    }
    public async Task DisposeAsync()
    {
        if (LibrarySystemContext != null)
        {
            await LibrarySystemContext.Database.EnsureDeletedAsync();
        }
        _serviceScope?.Dispose();
    }
}