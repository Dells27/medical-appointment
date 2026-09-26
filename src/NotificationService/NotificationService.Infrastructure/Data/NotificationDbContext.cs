using Microsoft.EntityFrameworkCore;
using NotificationService.Domain.Entities;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(DbContextOptions<NotificationDbContext> options) : base(options) { }

    public DbSet<Notification> NotificationLogs => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notification_logs");
            entity.HasKey(n => n.Id);

            entity.Property(n => n.Id).HasColumnName("id");
            entity.Property(n => n.eventType).HasColumnName("event_type").IsRequired();
            entity.Property(n => n.recipientEmail).HasColumnName("recipient_email").IsRequired();
            entity.Property(n => n.subject).HasColumnName("subject").IsRequired();
            entity.Property(n => n.status).HasColumnName("status").HasConversion<string>();
            entity.Property(n => n.errorMessages).HasColumnName("error_message");
            entity.Property(n => n.sentAt).HasColumnName("sent_at");
        });
    }
}