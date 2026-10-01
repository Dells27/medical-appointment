using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MedicalRecordService.Infrastructure.Data;

public class MedicalRecordDbContextFactory : IDesignTimeDbContextFactory<MedicalRecordDbContext>
{
    public MedicalRecordDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MedicalRecordDbContext>();

        optionsBuilder.UseNpgsql(
            "Host=127.0.0.1;Port=5432;Database=db_medical_records;Username=medic_admin;Password=medic_password");

        return new MedicalRecordDbContext(optionsBuilder.Options);
    }
}