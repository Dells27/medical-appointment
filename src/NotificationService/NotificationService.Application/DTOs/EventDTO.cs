namespace NotificationService.Application.DTOs
{
    // Mirrors of the events published by the Appointment Service (exchange "medic_events")

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
        public string cancelledByRole { get; set; } = string.Empty;
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
}
