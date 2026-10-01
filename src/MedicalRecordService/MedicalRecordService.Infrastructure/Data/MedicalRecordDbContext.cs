using MedicalRecordService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Infrastructure.Data;

public class MedicalRecordDbContext : DbContext
{
    public MedicalRecordDbContext(DbContextOptions<MedicalRecordDbContext> options) : base(options) { }

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MedicalRecord>(entity =>
        {
            entity.ToTable("medical_records");
            entity.HasKey(m => m.Id);

            entity.Property(m => m.Id).HasColumnName("id");
            entity.Property(m => m.appointmentId).HasColumnName("appointment_id");
            entity.Property(m => m.patientId).HasColumnName("patient_id");
            entity.Property(m => m.doctorId).HasColumnName("doctor_id");
            entity.Property(m => m.appointmentDate).HasColumnName("appointment_date");
            entity.Property(m => m.diagnosis).HasColumnName("diagnosis");
            entity.Property(m => m.treatment).HasColumnName("treatment");
            entity.Property(m => m.observations).HasColumnName("observations");
            entity.Property(m => m.isCompleted).HasColumnName("is_completed");
            entity.Property(m => m.createdAt).HasColumnName("created_at");
            entity.Property(m => m.updatedAt).HasColumnName("updated_at");

            entity.HasIndex(m => m.appointmentId).IsUnique();
        });
    }
}