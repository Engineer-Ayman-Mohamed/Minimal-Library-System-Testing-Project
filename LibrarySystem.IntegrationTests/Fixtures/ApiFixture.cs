using LibrarySystem.Data.Context;
using LibrarySystem.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LibrarySystem.IntegrationTests.Fixtures;

/// <summary>
///     Test fixture that hosts the API in-process with an in-memory database.
///     Implements <see cref="IAsyncLifetime" /> so xUnit manages setup and teardown.
/// </summary>
public class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private IServiceScope _serviceScope = null!;
    private const string IntegrationTestInMemoryDatabaseName = "IntegrationTestDb"; 
    public LibrarySystemContext LibrarySystemContext { get; private set; } = null!;

    /// <summary>Overrides the EF Core context to use an in-memory database.</summary>
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

    /// <summary>Creates a DI scope and exposes the in-memory context for seed data.</summary>
    public async Task InitializeAsync()
    {
        _serviceScope = Services.CreateScope();
        LibrarySystemContext = _serviceScope.ServiceProvider
            .GetRequiredService<LibrarySystemContext>();
    }

    /// <summary>Deletes the in-memory database and disposes the scope.</summary>
    public async Task DisposeAsync()
    {
        if (LibrarySystemContext != null)
        {
            await LibrarySystemContext.Database.EnsureDeletedAsync();
        }
        _serviceScope?.Dispose();
    }
}