using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LibrarySystem.Data.Context;

public class LibrarySystemContext : DbContext
{
    public LibrarySystemContext(DbContextOptions<LibrarySystemContext> options)
        : base(options) { }

    public DbSet<Book> Books => Set<Book>();
    public DbSet<Member> Members => Set<Member>();
    public DbSet<Loan> Loans => Set<Loan>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Library");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(LibrarySystemContext).Assembly);
    }
}