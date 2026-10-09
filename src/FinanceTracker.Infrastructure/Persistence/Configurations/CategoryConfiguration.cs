using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");
        
        builder.HasKey(c => c.Id);
        
        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(c => c.ColorHex)
            .IsRequired()
            .HasMaxLength(7)
            .HasDefaultValue("#808080");

        builder.Property(c => c.Type)
            .IsRequired();

        builder.Property(c => c.UserId)
            .IsRequired(false);
        
        builder.HasOne(c => c.User)
            .WithMany(u => u.Categories)
            .HasForeignKey(c => c.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(c => c.UserId);

        builder.HasIndex(c => new { c.UserId, c.Name, c.Type });
    }
}