using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PatientService.Domain.Entities;

namespace PatientService.Infrastructure.Data
{
    public class PatientDbContext : DbContext
    {

        public PatientDbContext(DbContextOptions<PatientDbContext> options) : base(options) { }

        public DbSet<Patient> Patients => Set<Patient>();
        public DbSet<MedicalDocument> MedicalDocuments => Set<MedicalDocument>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Patient>(entity =>
            {
                entity.ToTable("patients");
                entity.HasKey(p => p.id);

                entity.Property(p => p.id).HasColumnName("id");
                entity.Property(p => p.userId).HasColumnName("user_id");
                entity.Property(p => p.name).HasColumnName("full_name").IsRequired().HasMaxLength(255);
                entity.Property(p => p.email).HasColumnName("email").IsRequired().HasMaxLength(255);
                entity.Property(p => p.dateOfBirth).HasColumnName("date_of_birth");
                entity.Property(p => p.phoneNumber).HasColumnName("phone_number").HasMaxLength(20);
                entity.Property(p => p.bloodType).HasColumnName("blood_type").HasMaxLength(5);
                entity.Property(p => p.allergies).HasColumnName("allergies");
                entity.Property(p => p.isActive).HasColumnName("is_active");
                entity.Property(p => p.createdAt).HasColumnName("created_at");

                entity.HasMany(p => p.Document)
                      .WithOne()
                      .HasForeignKey(d => d.patientId)
                      .OnDelete(DeleteBehavior.Cascade);
                entity.Navigation(p => p.Document)
      .HasField("_documents")
      .UsePropertyAccessMode(PropertyAccessMode.Field);


                entity.HasIndex(p => p.userId).IsUnique();
            });

            modelBuilder.Entity<MedicalDocument>(entity =>
            {
                entity.ToTable("medical_documents");
                entity.HasKey(d => d.id);

                entity.Property(d => d.id).HasColumnName("id");
                entity.Property(d => d.patientId).HasColumnName("patient_id");
                entity.Property(d => d.fileName).HasColumnName("file_name").IsRequired();
                entity.Property(d => d.fileKey).HasColumnName("file_key").IsRequired();
                entity.Property(d => d.fileType).HasColumnName("file_type").IsRequired();
                entity.Property(d => d.fileSize).HasColumnName("file_size");
                entity.Property(d => d.documentType).HasColumnName("document_type").IsRequired();
                entity.Property(d => d.uploadedByRole).HasColumnName("uploaded_by_role").HasMaxLength(20)
                      .IsRequired().HasDefaultValue("Patient");
                entity.Property(d => d.uploadedByUserId).HasColumnName("uploaded_by_user_id");
                entity.Property(d => d.uploadedByName).HasColumnName("uploaded_by_name").HasMaxLength(255);
                entity.Property(d => d.appointmentId).HasColumnName("appointment_id");
                entity.Property(d => d.description).HasColumnName("description").HasMaxLength(500);
                entity.Property(d => d.uploadedAt).HasColumnName("uploaded_at");
            });
        }
    }
}
