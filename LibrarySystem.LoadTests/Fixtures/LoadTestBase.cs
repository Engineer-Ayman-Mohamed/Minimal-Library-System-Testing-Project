using LibrarySystem.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.LoadTests.Fixtures;

public class LoadTestBase : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IServiceScope _serviceScope = null!;
    private const string LoadTestInMemoryDatabaseName = "LoadTestDb"; 
    public HttpClient Client { get; private set; } = null!;
    public LibrarySystemContext LibrarySystemContext { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("LoadTest");
        builder.ConfigureTestServices(services =>
        {
            services.AddDbContext<LibrarySystemContext>(options =>
            {
                options.UseInMemoryDatabase(LoadTestInMemoryDatabaseName);
            });
        });
    }
    public async Task InitializeAsync()
    {
        _serviceScope = Services.CreateScope();
        LibrarySystemContext = _serviceScope.ServiceProvider
            .GetRequiredService<LibrarySystemContext>();
        Client = CreateClient();
    }
    public async Task DisposeAsync()
    {
        Client?.Dispose();
        if (LibrarySystemContext is not null)
        {
            await LibrarySystemContext.Database.EnsureDeletedAsync();
        }
        _serviceScope?.Dispose();
    }
}