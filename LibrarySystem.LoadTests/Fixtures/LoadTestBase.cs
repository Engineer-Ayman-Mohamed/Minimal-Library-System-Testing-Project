using LibrarySystem.Data.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.LoadTests.Fixtures;

/// <summary>
///     Test fixture for load, stress, and spike tests.
///     Hosts the API in-process with an in-memory database and exposes an <see cref="HttpClient" />.
/// </summary>
public class LoadTestBase : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IServiceScope _serviceScope = null!;
    private const string LoadTestInMemoryDatabaseName = "LoadTestDb"; 
    public HttpClient Client { get; private set; } = null!;
    public LibrarySystemContext LibrarySystemContext { get; private set; } = null!;

    /// <summary>Sets the environment to "LoadTest" and uses an in-memory database.</summary>
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

    /// <summary>Creates the DI scope, in-memory context, and HTTP client.</summary>
    public async Task InitializeAsync()
    {
        _serviceScope = Services.CreateScope();
        LibrarySystemContext = _serviceScope.ServiceProvider
            .GetRequiredService<LibrarySystemContext>();
        Client = CreateClient();
    }

    /// <summary>Disposes the HTTP client, deletes the in-memory database, and cleans up the scope.</summary>
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