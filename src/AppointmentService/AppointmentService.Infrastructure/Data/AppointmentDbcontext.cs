using AppointmentService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AppointmentService.Infrastructure.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options) : base(options) { }

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.ToTable("appointments");
            entity.HasKey(a => a.Id);

            entity.Property(a => a.Id).HasColumnName("id");
            entity.Property(a => a.patientId).HasColumnName("patient_id");
            entity.Property(a => a.doctorId).HasColumnName("doctor_id");
            entity.Property(a => a.patientName).HasColumnName("patient_name").HasMaxLength(255);
            entity.Property(a => a.patientEmail).HasColumnName("patient_email").HasMaxLength(255);
            entity.Property(a => a.appointmentDate).HasColumnName("appointment_date");
            entity.Property(a => a.appointmentTime).HasColumnName("appointment_time");
            entity.Property(a => a.status)
                  .HasColumnName("status")
                  .HasConversion<string>(); // store the enum as readable text
            entity.Property(a => a.reason).HasColumnName("reason");
            entity.Property(a => a.cancellationReason).HasColumnName("cancellation_reason");
            entity.Property(a => a.cancelledByRole).HasColumnName("cancelled_by_role").HasMaxLength(20);
            entity.Property(a => a.rescheduledAt).HasColumnName("rescheduled_at");
            entity.Property(a => a.createdAt).HasColumnName("created_at");
            entity.Property(a => a.updatedAt).HasColumnName("updated_at");

            // Unique index — prevents race conditions:
            // a doctor cannot have two active appointments at the same time
            entity.HasIndex(a => new { a.doctorId, a.appointmentDate, a.appointmentTime })
                  .IsUnique()
                  .HasFilter("status != 'Cancelled'");

            entity.HasIndex(a => new { a.doctorId, a.patientId });
        });
    }
}
