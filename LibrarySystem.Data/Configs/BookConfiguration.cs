using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibrarySystem.Data.Configs;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.Id);
        builder.HasIndex(b => b.ISBN).IsUnique();
        builder.Property(b => b.Title).IsRequired().HasMaxLength(200);
        builder.Property(b => b.Author).IsRequired();
        builder.Property(b => b.ISBN).IsRequired();
    }
}