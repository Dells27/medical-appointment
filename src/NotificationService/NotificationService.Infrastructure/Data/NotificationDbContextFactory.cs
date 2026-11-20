using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        optionsBuilder.UseNpgsql(
            Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING") ?? "Host=localhost;Port=5432;Database=db_notifications;Username=medic_admin");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}