using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


    namespace NotificationService.Application.DTOs
    {
        public class AppointmentCreatedEvent
        {
            public Guid appointmentId { get; set; }
            public Guid patientId { get; set; }
            public Guid doctorId { get; set; }
            public DateTime appointmentDate { get; set; }
            public TimeOnly appointmentTime { get; set; }
            public string PatientEmail { get; set; } = string.Empty;
        }

        public class AppointmentCancelledEvent
        {
            public Guid appointmentId { get; set; }
            public Guid patientId { get; set; }
            public Guid doctorId { get; set; }
            public string reason { get; set; } = string.Empty;
        }
    }

