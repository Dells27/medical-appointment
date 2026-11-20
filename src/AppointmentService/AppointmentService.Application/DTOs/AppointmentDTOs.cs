using System.Text.Json.Serialization;

namespace AppointmentService.Application.DTOs
{
    // ============================================================
    // Requests
    // ============================================================

    public class CreateAppointmentRequest
    {
        // Filled from the JWT, never from the client
        [JsonIgnore] public Guid PatientID { get; set; }
        [JsonIgnore] public string PatientEmail { get; set; } = string.Empty;
        [JsonIgnore] public string PatientName { get; set; } = string.Empty;

        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string? reason { get; set; }
    }

    public class CancelAppointmentRequest
    {
        public string reason { get; set; } = string.Empty;
    }

    public class RescheduleAppointmentRequest
    {
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
    }

    // ============================================================
    // Responses
    // ============================================================

    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public string? patientName { get; set; }
        public string? patientEmail { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string status { get; set; } = string.Empty;
        public string? reason { get; set; }
        public string? cancellationReason { get; set; }
        public string? cancelledByRole { get; set; }
        public DateTime? rescheduledAt { get; set; }
        public DateTime createdAt { get; set; }
    }

    public class AvailableSlotsResponse
    {
        public Guid doctorId { get; set; }
        public DateOnly date { get; set; }
        public int slotMinutes { get; set; }
        public List<TimeOnly> slots { get; set; } = new();
    }

    // One row of the doctor's patient list
    public class DoctorPatientResponse
    {
        public Guid patientId { get; set; }
        public string? patientName { get; set; }
        public string? patientEmail { get; set; }
        public int completedVisits { get; set; }
        public int totalAppointments { get; set; }
        public DateTime? lastVisit { get; set; }
        public DateTime? nextAppointmentDate { get; set; }
        public TimeOnly? nextAppointmentTime { get; set; }
    }

    public class MonthlyCountResponse
    {
        public string month { get; set; } = string.Empty; // "2026-11"
        public int completed { get; set; }
        public int cancelled { get; set; }
    }

    public class DoctorStatsResponse
    {
        public int totalAppointments { get; set; }
        public Dictionary<string, int> byStatus { get; set; } = new();
        public int patientsAttended { get; set; }   // unique patients with a completed visit
        public int upcoming { get; set; }           // scheduled or confirmed, from today on
        public int today { get; set; }
        public int pendingConfirmation { get; set; }
        public double cancellationRate { get; set; } // 0..1
        public List<MonthlyCountResponse> lastSixMonths { get; set; } = new();
    }

    // ============================================================
    // Events published to RabbitMQ (exchange "medic_events")
    // ============================================================

    public class AppointmentCreatedEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string? reason { get; set; }
        public string PatientEmail { get; set; } = string.Empty;
        public string patientName { get; set; } = string.Empty;
        public string doctorEmail { get; set; } = string.Empty;
        public string doctorName { get; set; } = string.Empty;
    }

    public class AppointmentConfirmedEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string patientEmail { get; set; } = string.Empty;
        public string patientName { get; set; } = string.Empty;
        public string doctorName { get; set; } = string.Empty;
    }

    public class AppointmentCancelledEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string reason { get; set; } = string.Empty;
        public string cancelledByRole { get; set; } = string.Empty; // "Patient" | "Doctor"
        public string patientEmail { get; set; } = string.Empty;
        public string patientName { get; set; } = string.Empty;
        public string doctorEmail { get; set; } = string.Empty;
        public string doctorName { get; set; } = string.Empty;
    }

    public class AppointmentRescheduledEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime previousDate { get; set; }
        public TimeOnly previousTime { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string patientEmail { get; set; } = string.Empty;
        public string patientName { get; set; } = string.Empty;
        public string doctorEmail { get; set; } = string.Empty;
        public string doctorName { get; set; } = string.Empty;
    }

    public class AppointmentCompletedEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
    }
}
