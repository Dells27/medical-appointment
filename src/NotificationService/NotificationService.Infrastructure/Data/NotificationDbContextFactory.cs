using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NotificationService.Infrastructure.Data;

public class NotificationDbContextFactory : IDesignTimeDbContextFactory<NotificationDbContext>
{
    public NotificationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<NotificationDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=localhost;Port=5432;Database=db_notifications;Username=medic_admin;Password=medic_password");

        return new NotificationDbContext(optionsBuilder.Options);
    }
}