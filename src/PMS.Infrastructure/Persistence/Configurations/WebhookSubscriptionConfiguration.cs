using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PMS.Domain.Users;
using PMS.Domain.Webhooks;

namespace PMS.Infrastructure.Persistence.Configurations;

public sealed class WebhookSubscriptionConfiguration : IEntityTypeConfiguration<WebhookSubscription>
{
    public void Configure(EntityTypeBuilder<WebhookSubscription> builder)
    {
        builder.ToTable("webhook_subscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Url).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.EventType).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Secret).HasMaxLength(512);
        builder.HasIndex(x => new { x.UserId, x.EventType, x.Url }).IsUnique();
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
