using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace AppointmentService.Application.DTOs
{
    //Request para agender cita
    public class CreateAppointmentRequest
    {
        [JsonIgnore]
        public Guid PatientID { get; set; }
        [JsonIgnore]
        public string PatientEmail { get; set; } = string.Empty;

        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string? reason { get; set; }



    }


   //Request para cancelar cita

    public class CancelAppointmentRequest
    {
        public string reason { get; set; } = string.Empty;
    }

    //Response de la cita
    public class AppointmentResponse
    {
        public Guid Id { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
        public TimeOnly appointmentTime { get; set; }
        public string status { get; set; } = string.Empty;
        public string? reason { get; set; }
        public string? cancellationReason { get; set; }
        public DateTime createdAt { get; set; }
    }

    //Evento que se publica en RabbitMQ cuando se agenda una cita
    public class AppointmentCreatedEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
    public TimeOnly appointmentTime{ get; set; }
        public string PatientEmail { get; set; } = string.Empty;

    }


    //Evento que se publica cuando se cancela una cita
    public class AppointmentCancelledEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public string reason { get; set; } = string.Empty;
    }


    public class AppointmentCompletedEvent
    {
        public Guid appointmentId { get; set; }
        public Guid patientId { get; set; }
        public Guid doctorId { get; set; }
        public DateTime appointmentDate { get; set; }
    }
}
