using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedicalRecordService.Application.DTOs
{
  // Evento que llega de RabbitMQ desde Appointment Service
public class AppointmentCompletedEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
    }

    // Request para que el médico llene las notas clínicas
    public class FillNotesRequest
    {
        public string diagnosis { get; set; } = string.Empty;
        public string Treatment { get; set; } = string.Empty;
        public string? observations { get; set; }
    }

    public class UpdateMedicalBackgroundRequest
    {
        public string? chronicConditions { get; set; }
        public string? surgeries { get; set; }
        public string? currentMedications { get; set; }
        public string? familyHistory { get; set; }
        public string? allergies { get; set; }
        public string? habits { get; set; }
        public string? notes { get; set; }
    }

    public class MedicalBackgroundResponse
    {
        public Guid patientId { get; set; }
        public string? chronicConditions { get; set; }
        public string? surgeries { get; set; }
        public string? currentMedications { get; set; }
        public string? familyHistory { get; set; }
        public string? allergies { get; set; }
        public string? habits { get; set; }
        public string? notes { get; set; }
        public Guid? updatedByDoctorId { get; set; }
        public DateTime? updatedAt { get; set; } // null when nothing has been recorded yet
    }

    // Clinical record of one consultation
    public class MedicalRecordResponse
    {
        public Guid Id { get; set; }
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public string? diagnosis { get; set; }
        public string? treatment { get; set; }
        public string? observations { get; set; }
        public bool isCompleted { get; set; }
        public DateTime createdAt { get; set; }
    }
}
