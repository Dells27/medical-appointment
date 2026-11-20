using MedicalRecordService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MedicalRecordService.Infrastructure.Data;

public class MedicalRecordDbContext : DbContext
{
    public MedicalRecordDbContext(DbContextOptions<MedicalRecordDbContext> options) : base(options) { }

    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<MedicalBackground> MedicalBackgrounds => Set<MedicalBackground>();

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
            entity.HasIndex(m => m.patientId);
        });

        modelBuilder.Entity<MedicalBackground>(entity =>
        {
            entity.ToTable("medical_backgrounds");
            entity.HasKey(b => b.Id);

            entity.Property(b => b.Id).HasColumnName("id");
            entity.Property(b => b.patientId).HasColumnName("patient_id");
            entity.Property(b => b.chronicConditions).HasColumnName("chronic_conditions");
            entity.Property(b => b.surgeries).HasColumnName("surgeries");
            entity.Property(b => b.currentMedications).HasColumnName("current_medications");
            entity.Property(b => b.familyHistory).HasColumnName("family_history");
            entity.Property(b => b.allergies).HasColumnName("allergies");
            entity.Property(b => b.habits).HasColumnName("habits");
            entity.Property(b => b.notes).HasColumnName("notes");
            entity.Property(b => b.updatedByDoctorId).HasColumnName("updated_by_doctor_id");
            entity.Property(b => b.createdAt).HasColumnName("created_at");
            entity.Property(b => b.updatedAt).HasColumnName("updated_at");

            entity.HasIndex(b => b.patientId).IsUnique();
        });
    }
}
