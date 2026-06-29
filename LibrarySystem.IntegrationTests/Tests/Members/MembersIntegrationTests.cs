using System.Net;
using System.Net.Http.Json;
using LibrarySystem.API.DTOs;
using LibrarySystem.Data.Entities;
using LibrarySystem.IntegrationTests.Fixtures;
using Shouldly;

namespace LibrarySystem.IntegrationTests.Tests.Members;

public class MembersIntegrationTests : IClassFixture<ApiFixture>, IAsyncLifetime
{
    private readonly ApiFixture _fixture;
    private readonly HttpClient _client;

    public MembersIntegrationTests(ApiFixture fixture)
    {
        _fixture = fixture;
        _client = fixture.CreateClient();
    }

    public async Task InitializeAsync()
    {
        _fixture.LibrarySystemContext.Loans.RemoveRange(_fixture.LibrarySystemContext.Loans);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Books.RemoveRange(_fixture.LibrarySystemContext.Books);
        _fixture.LibrarySystemContext.Members.RemoveRange(_fixture.LibrarySystemContext.Members);
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Books.AddRange(
            new Book { Id = 1, Title = "Test Book 1", Author = "Author 1", ISBN = "9780123456789", TotalCopies = 3, AvailableCopies = 2 },
            new Book { Id = 2, Title = "Test Book 2", Author = "Author 2", ISBN = "9780123456790", TotalCopies = 1, AvailableCopies = 0 }
        );
        _fixture.LibrarySystemContext.Members.AddRange(
            new Member { Id = 1, FullName = "John Doe", Email = "john@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 },
            new Member { Id = 2, FullName = "Jane Smith", Email = "jane@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 0 },
            new Member { Id = 3, FullName = "Bob Wilson", Email = "bob@test.com", MembershipExpiryDate = DateTime.Today.AddYears(1), OutstandingFine = 15.50m }
        );
        await _fixture.LibrarySystemContext.SaveChangesAsync();

        _fixture.LibrarySystemContext.Loans.AddRange(
            new Data.Entities.Loan { Id = 1, BookId = 1, MemberId = 1, BorrowedAt = DateTime.Today, DueDate = DateTime.Today.AddDays(14), ReturnedAt = null, FineAmount = 0 },
            new Data.Entities.Loan { Id = 2, BookId = 2, MemberId = 1, BorrowedAt = DateTime.Today.AddDays(-20), DueDate = DateTime.Today.AddDays(-6), ReturnedAt = DateTime.Today, FineAmount = 0 }
        );
        await _fixture.LibrarySystemContext.SaveChangesAsync();
    }
    public Task DisposeAsync() => Task.CompletedTask;
    
    [Fact]
    public async Task GetById_ExistingMember_ReturnsOkWithMember()
    {
        var response = await _client.GetAsync("api/v1/Members/1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.Id.ShouldBe(1);
        member.FullName.ShouldBe("John Doe");
        member.Email.ShouldBe("john@test.com");
        member.OutstandingFine.ShouldBe(0);
    }
    
    [Fact]
    public async Task GetById_NonExistentMember_ReturnsNotFound()
    {
        var response = await _client.GetAsync("api/v1/Members/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task GetById_MemberWithActiveLoans_ReturnsCorrectActiveLoanCount()
    {
        var response = await _client.GetAsync("api/v1/Members/1");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.ActiveLoanCount.ShouldBe(1);
    }
    
    [Fact]
    public async Task GetById_MemberWithNoLoans_ReturnsZeroActiveLoanCount()
    {
        var response = await _client.GetAsync("api/v1/Members/2");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.ActiveLoanCount.ShouldBe(0);
    }
    
    [Fact]
    public async Task GetById_MemberWithOutstandingFine_ReturnsCorrectFineAmount()
    {
        var response = await _client.GetAsync("api/v1/Members/3");

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.OutstandingFine.ShouldBe(15.50m);
    }
    
    [Fact]
    public async Task Create_ValidMember_ReturnsCreatedWithMember()
    {
        var dto = new CreateMemberDto
        {
            FullName = "New Member",
            Email = "newmember@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var response = await _client.PostAsJsonAsync("api/v1/Members", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.FullName.ShouldBe(dto.FullName);
        member.Email.ShouldBe(dto.Email);
        member.MembershipExpiryDate.ShouldBe(dto.MembershipExpiryDate);
        member.ActiveLoanCount.ShouldBe(0);
        member.OutstandingFine.ShouldBe(0);
    }
    
    [Fact]
    public async Task Create_DuplicateEmail_ReturnsConflict()
    {
        var dto = new CreateMemberDto
        {
            FullName = "Another John",
            Email = "john@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var response = await _client.PostAsJsonAsync("api/v1/Members", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }
    
    [Fact]
    public async Task Create_InvalidEmail_ReturnsBadRequest()
    {
        var dto = new CreateMemberDto
        {
            FullName = "Bad Member",
            Email = "not-a-valid-email",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var response = await _client.PostAsJsonAsync("api/v1/Members", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_MissingFullName_ReturnsBadRequest()
    {
        var dto = new CreateMemberDto
        {
            FullName = "",
            Email = "valid@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var response = await _client.PostAsJsonAsync("api/v1/Members", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Create_ValidMember_IsRetrievableAfterCreation()
    {
        var dto = new CreateMemberDto
        {
            FullName = "Retrievable Member",
            Email = "retrievable@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var createResponse = await _client.PostAsJsonAsync("api/v1/Members", dto);
        createResponse.StatusCode.ShouldBe(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<MemberDto>();
        created.ShouldNotBeNull();

        var getResponse = await _client.GetAsync($"api/v1/Members/{created.Id}");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.OK);

        var retrieved = await getResponse.Content.ReadFromJsonAsync<MemberDto>();
        retrieved.ShouldNotBeNull();
        retrieved.Id.ShouldBe(created.Id);
        retrieved.FullName.ShouldBe(dto.FullName);
        retrieved.Email.ShouldBe(dto.Email);
    }
    
    [Fact]
    public async Task Patch_ExistingMember_ReturnsOkWithUpdatedMember()
    {
        var dto = new UpdateMemberDto
        {
            FullName = "Updated John",
            Email = "updatedjohn@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(2)
        };

        var response = await _client.PatchAsJsonAsync("api/v1/Members/1", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.Id.ShouldBe(1);
        member.FullName.ShouldBe(dto.FullName);
        member.Email.ShouldBe(dto.Email);
        member.MembershipExpiryDate.ShouldBe(dto.MembershipExpiryDate);
    }
    
    [Fact]
    public async Task Patch_NonExistentMember_ReturnsNotFound()
    {
        var dto = new UpdateMemberDto
        {
            FullName = "Ghost Member",
            Email = "ghost@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var response = await _client.PatchAsJsonAsync("api/v1/Members/999", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Patch_ExistingMember_ActiveLoanCountRemainsCorrect()
    {
        var dto = new UpdateMemberDto
        {
            FullName = "Updated John",
            Email = "updatedjohn@test.com",
            MembershipExpiryDate = DateTime.Today.AddYears(2)
        };

        var response = await _client.PatchAsJsonAsync("api/v1/Members/1", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);

        var member = await response.Content.ReadFromJsonAsync<MemberDto>();
        member.ShouldNotBeNull();
        member.ActiveLoanCount.ShouldBe(1);
    }
    
    [Fact]
    public async Task Patch_InvalidEmail_ReturnsBadRequest()
    {
        var dto = new UpdateMemberDto
        {
            FullName = "John Doe",
            Email = "not-a-valid-email",
            MembershipExpiryDate = DateTime.Today.AddYears(1)
        };

        var response = await _client.PatchAsJsonAsync("api/v1/Members/1", dto);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }
    
    [Fact]
    public async Task Delete_ExistingMember_ReturnsNoContent()
    {
        var response = await _client.DeleteAsync("api/v1/Members/2");

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }
    
    [Fact]
    public async Task Delete_NonExistentMember_ReturnsNotFound()
    {
        var response = await _client.DeleteAsync("api/v1/Members/999");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
    
    [Fact]
    public async Task Delete_ExistingMember_IsNoLongerRetrievable()
    {
        var deleteResponse = await _client.DeleteAsync("api/v1/Members/3");
        deleteResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync("api/v1/Members/3");
        getResponse.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}