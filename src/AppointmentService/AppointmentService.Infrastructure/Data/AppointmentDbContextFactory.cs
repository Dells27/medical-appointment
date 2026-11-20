using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AppointmentService.Infrastructure.Data;

// Este factory le permite a "dotnet ef" crear el DbContext
// sin necesidad de levantar Redis, RabbitMQ ni el resto del Program.cs
public class AppointmentDbContextFactory : IDesignTimeDbContextFactory<AppointmentDbContext>
{
    public AppointmentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppointmentDbContext>();

        optionsBuilder.UseNpgsql(
            Environment.GetEnvironmentVariable("DESIGN_TIME_CONNECTION_STRING") ?? "Host=localhost;Port=5432;Database=db_appointments;Username=medic_admin");

        return new AppointmentDbContext(optionsBuilder.Options);
    }
}