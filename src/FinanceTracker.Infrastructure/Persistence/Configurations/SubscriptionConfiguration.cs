using FinanceTracker.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinanceTracker.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("Subscriptions");

        builder.HasKey(s => s.Id);
        
        builder.Property(s => s.Title)
            .HasMaxLength(100)
            .IsRequired();
        
        builder.Property(s => s.Amount)
            .HasColumnType("numeric(18,2)")
            .IsRequired();
        
        builder.Property(s => s.BillingPeriod)
            .IsRequired();
        
        builder.Property(s => s.NextPaymentDate)
            .HasColumnType("date")
            .IsRequired();
        
        builder.HasOne(s => s.Category)
            .WithMany(c => c.Subscriptions)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(s => s.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasIndex(s => new { s.IsActive, NextPaymentDate = s.NextPaymentDate})
            .HasDatabaseName("IX_Subscriptions_IsActive_NextPaymentDate");
    }
}
