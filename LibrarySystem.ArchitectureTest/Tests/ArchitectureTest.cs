using System.Reflection;
using LibrarySystem.API.Controllers.V1.Books;
using LibrarySystem.Data.Entities;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Data.Repositories.Repos;
using LibrarySystem.Services.services;
using LibrarySystem.Services.services.interfaces;
using NetArchTest.Rules;

namespace LibrarySystem.ArchitectureTest.Tests;

/// <summary>Architecture tests that enforce layer dependencies, naming conventions, and namespace rules across the solution.</summary>
public class ArchitectureTest
{
    private static readonly Assembly DataAssembly = typeof(Book).Assembly;
    private static readonly Assembly ServicesAssembly = typeof(BookService).Assembly;
    private static readonly Assembly ApiAssembly = typeof(BooksController).Assembly;
    
    /// <summary>Ensures the Data layer has no dependency on the Services layer.</summary>
    [Test]
    public void DataLayer_ShouldNotHaveDependencyOn_ServicesLayer()
    {
        var result = Types
            .InAssembly(DataAssembly)
            .ShouldNot()
            .HaveDependencyOn("LibrarySystem.Services")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Data layer depends on Services: {FailingNames(result)}");
    }
    
    /// <summary>Ensures the Data layer has no dependency on the API layer.</summary>
    [Test]
    public void DataLayer_ShouldNotHaveDependencyOn_ApiLayer()
    {
        var result = Types
            .InAssembly(DataAssembly)
            .ShouldNot()
            .HaveDependencyOn("LibrarySystem.API")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Data layer depends on API: {FailingNames(result)}");
    }
    
    /// <summary>Ensures the Services layer has no dependency on the API layer.</summary>
    [Test]
    public void ServicesLayer_ShouldNotHaveDependencyOn_ApiLayer()
    {
        var result = Types
            .InAssembly(ServicesAssembly)
            .ShouldNot()
            .HaveDependencyOn("LibrarySystem.API")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Services layer depends on API: {FailingNames(result)}");
    }
    
    /// <summary>Ensures all interfaces follow the naming convention of starting with 'I'.</summary>
    [Test]
    public void Interfaces_ShouldStartWithI()
    {
        var result = Types
            .InAssemblies(new[] { DataAssembly, ServicesAssembly })
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Interfaces not starting with I: {FailingNames(result)}");
    }
    
    /// <summary>Ensures repository implementations reside in the Data.Repositories namespace.</summary>
    [Test]
    public void RepositoryClasses_ShouldBeInRepositoriesNamespace()
    {
        var result = Types
            .InAssembly(DataAssembly)
            .That()
            .ImplementInterface(typeof(IBookRepository))
            .Should()
            .ResideInNamespace("LibrarySystem.Data.Repositories")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Repositories outside Repositories namespace: {FailingNames(result)}");
    }
    
    /// <summary>Ensures service implementations reside in the Services namespace.</summary>
    [Test]
    public void ServiceClasses_ShouldBeInServicesNamespace()
    {
        var result = Types
            .InAssembly(ServicesAssembly)
            .That()
            .ImplementInterface(typeof(IBookService))
            .Should()
            .ResideInNamespace("LibrarySystem.Services")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Services outside Services namespace: {FailingNames(result)}");
    }
    
    /// <summary>Ensures controller classes reside in the API.Controllers namespace.</summary>
    [Test]
    public void ControllerClasses_ShouldBeInControllersNamespace()
    {
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .HaveNameEndingWith("Controller")
            .Should()
            .ResideInNamespace("LibrarySystem.API.Controllers")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Controllers outside Controllers namespace: {FailingNames(result)}");
    }
    
    /// <summary>Ensures all service classes implement at least one interface.</summary>
    [Test]
    public void AllServices_ShouldImplementAnInterface()
    {
        var serviceTypes = new[]
        {
            typeof(BookService), typeof(MemberService), typeof(LoanService)
        };

        var failures = serviceTypes
            .Where(t => t.GetInterfaces().Length == 0)
            .Select(t => t.FullName)
            .ToList();

        Assert.That(failures, Is.Empty,
            $"Services without interface: {string.Join(", ", failures)}");
    }
    
    /// <summary>Ensures all repository classes implement at least one interface.</summary>
    [Test]
    public void AllRepositories_ShouldImplementAnInterface()
    {
        var repoTypes = new[]
        {
            typeof(BookRepository), typeof(MemberRepository), typeof(LoanRepository)
        };

        var failures = repoTypes
            .Where(t => t.GetInterfaces().Length == 0)
            .Select(t => t.FullName)
            .ToList();

        Assert.That(failures, Is.Empty,
            $"Repositories without interface: {string.Join(", ", failures)}");
    }
    
    /// <summary>Ensures service interfaces are defined in the Services assembly.</summary>
    [Test]
    public void ServiceInterfaces_ShouldBeInServicesAssembly()
    {
        var result = Types
            .InAssembly(ServicesAssembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameStartingWith("I")
            .And()
            .HaveNameEndingWith("Service")
            .Should()
            .ResideInNamespace("LibrarySystem.Services")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Service interfaces outside Services: {FailingNames(result)}");
    }
    
    /// <summary>Ensures repository interfaces are defined in the Data assembly.</summary>
    [Test]
    public void RepositoryInterfaces_ShouldBeInDataAssembly()
    {
        var result = Types
            .InAssembly(DataAssembly)
            .That()
            .AreInterfaces()
            .And()
            .HaveNameStartingWith("I")
            .And()
            .HaveNameEndingWith("Repository")
            .Should()
            .ResideInNamespace("LibrarySystem.Data.Repositories")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Repository interfaces outside Data: {FailingNames(result)}");
    }
    
    /// <summary>Ensures domain entities live exclusively in the Data assembly.</summary>
    [Test]
    public void DomainEntities_ShouldBeInDataAssembly_Only()
    {
        var entityTypes = new[] { typeof(Book), typeof(Member), typeof(Loan) };

        foreach (var entity in entityTypes)
        {
            Assert.That(entity.Assembly, Is.EqualTo(DataAssembly),
                $"{entity.Name} should live in Data assembly, not {entity.Assembly.GetName().Name}");
        }
    }
    
    /// <summary>Ensures the API layer does not directly reference domain entities outside of infrastructure namespaces.</summary>
    [Test]
    public void ApiLayer_ShouldNotReferenceDomainEntities_ExceptInfrastructure()
    {
        var result = Types
            .InAssembly(ApiAssembly)
            .That()
            .DoNotResideInNamespace("LibrarySystem.API.Mappings")
            .And()
            .DoNotResideInNamespace("LibrarySystem.API.Seeders")
            .And()
            .DoNotResideInNamespace("LibrarySystem.API.Controllers")
            .ShouldNot()
            .HaveDependencyOn("LibrarySystem.Data.Entities")
            .GetResult();

        Assert.That(result.IsSuccessful,
            $"Non-infrastructure API types reference domain entities: {FailingNames(result)}");
    }
    private static string FailingNames(TestResult result)
        => string.Join(", ", result.FailingTypeNames ?? []);
}