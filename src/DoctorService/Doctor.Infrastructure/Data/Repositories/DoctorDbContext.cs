using Microsoft.EntityFrameworkCore;
using DoctorService.Domain.Entities;

namespace DoctorService.Infrastructure.Data.Repositories
{
    public class DoctorDbContext : DbContext
    {
        public DoctorDbContext(DbContextOptions<DoctorDbContext> options) : base(options) { }

        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<AvailabilitySchedule> Schedules => Set<AvailabilitySchedule>();
        public DbSet<TimeOff> TimeOffs => Set<TimeOff>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Doctor>(entity =>
            {
                entity.ToTable("doctors");

                entity.HasKey(d => d.Id);

                entity.Property(d => d.Id).HasColumnName("id");
                entity.Property(d => d.UserId).HasColumnName("user_id");
                entity.Property(d => d.Name).HasColumnName("Name").IsRequired().HasMaxLength(255);
                entity.Property(d => d.Email).HasColumnName("email").IsRequired().HasMaxLength(255);
                entity.Property(d => d.Specialty).HasColumnName("specialty").IsRequired().HasMaxLength(100);
                entity.Property(d => d.LicenseNumber).HasColumnName("license_number").IsRequired().HasMaxLength(50);
                entity.Property(d => d.PhotoUrl).HasColumnName("photo_url");
                entity.Property(d => d.Bio).HasColumnName("bio");
                entity.Property(d => d.IsActive).HasColumnName("is_active");
                entity.Property(d => d.CreatedAt).HasColumnName("created_at");

                // A user can only have one doctor profile
                entity.HasIndex(d => d.UserId).IsUnique();
                entity.HasIndex(d => d.Email).IsUnique();

                entity.HasMany(d => d.Schedules)
                      .WithOne()
                      .HasForeignKey(s => s.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Navigation(d => d.Schedules)
                      .HasField("_schedules")
                      .UsePropertyAccessMode(PropertyAccessMode.Field);

                entity.HasMany(d => d.TimeOffs)
                      .WithOne()
                      .HasForeignKey(t => t.DoctorId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Navigation(d => d.TimeOffs)
                      .HasField("_timeOffs")
                      .UsePropertyAccessMode(PropertyAccessMode.Field);
            });


            modelBuilder.Entity<AvailabilitySchedule>(entity =>
            {
                entity.ToTable("availability_schedules");

                entity.HasKey(s => s.Id);

                entity.Property(s => s.Id).HasColumnName("id");
                entity.Property(s => s.DoctorId).HasColumnName("doctor_id");
                entity.Property(s => s.DayOfWeek).HasColumnName("day_of_week");
                entity.Property(s => s.StartTime).HasColumnName("start_time");
                entity.Property(s => s.EndTime).HasColumnName("end_time");
                entity.Property(s => s.IsActive).HasColumnName("is_active");
            });

            modelBuilder.Entity<TimeOff>(entity =>
            {
                entity.ToTable("time_offs");

                entity.HasKey(t => t.Id);

                entity.Property(t => t.Id).HasColumnName("id");
                entity.Property(t => t.DoctorId).HasColumnName("doctor_id");
                entity.Property(t => t.Type).HasColumnName("type").HasConversion<string>().HasMaxLength(20);
                entity.Property(t => t.StartDate).HasColumnName("start_date");
                entity.Property(t => t.EndDate).HasColumnName("end_date");
                entity.Property(t => t.StartTime).HasColumnName("start_time");
                entity.Property(t => t.EndTime).HasColumnName("end_time");
                entity.Property(t => t.Reason).HasColumnName("reason").HasMaxLength(500);
                entity.Property(t => t.CreatedAt).HasColumnName("created_at");

                entity.HasIndex(t => new { t.DoctorId, t.StartDate, t.EndDate });
            });

        }

    }
}
