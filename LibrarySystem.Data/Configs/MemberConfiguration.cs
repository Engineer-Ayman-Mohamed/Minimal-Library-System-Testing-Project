using LibrarySystem.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibrarySystem.Data.Configs;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);
        builder.HasIndex(m => m.Email).IsUnique();
        builder.Property(m => m.FullName).IsRequired();
        builder.Property(m => m.Email).IsRequired();
        builder.Property(m => m.OutstandingFine).HasDefaultValue(0);
    }
}