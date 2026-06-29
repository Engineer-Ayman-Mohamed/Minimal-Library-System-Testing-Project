using Asp.Versioning;
using LibrarySystem.API.Mappings;
using LibrarySystem.API.Seeders;
using LibrarySystem.Data.Context;
using LibrarySystem.Data.Repositories.Interfaces;
using LibrarySystem.Data.Repositories.Repos;
using LibrarySystem.Services.services;
using LibrarySystem.Services.services.interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0); //? default version
        options.AssumeDefaultVersionWhenUnspecified = true; //? accept request without specify api version (problem when there is more than one version)
        options.ReportApiVersions = true; //? add informations about the api version supported/deprecated with no docs
        options.ApiVersionReader = new UrlSegmentApiVersionReader(); //? reading the api version from the URL another ways(query, header)
    }) //? this is integration for the api versioning in swagger for documentation in swagger
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV"; //? v -> for the version number, VVV is the formatting of the version number
        options.SubstituteApiVersionInUrl = true; //? tells swagger to replace the version in Route(template) with actual version number
    });

//if (!builder.Environment.IsEnvironment("IntegrationTest"))
//{
//    builder.Services.AddDbContext<LibrarySystemContext>(options =>
//    {
//        options.UseSqlServer(builder.Configuration.GetConnectionString("Default"))
//            .EnableSensitiveDataLogging()
//            .LogTo(Console.WriteLine, LogLevel.Information);
//    });
//}
builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IMemberRepository, MemberRepository>();
builder.Services.AddScoped<ILoanRepository, LoanRepository>();

builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<ILoanService, LoanService>();

builder.Services.AddScoped<DataSeeder>();

builder.Services.AddSwaggerGen();

builder.Services.AddAutoMapper(configAction: config =>
{
    config.AddProfile<MappingsProfile>();
});
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//if (!app.Environment.IsEnvironment("IntegrationTest"))
//{
//    using (var scope = app.Services.CreateScope())
//    {
//        var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
//        await seeder.SeedAsync();
//    }
//}
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
