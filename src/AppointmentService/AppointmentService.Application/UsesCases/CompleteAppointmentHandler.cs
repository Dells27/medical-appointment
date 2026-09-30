using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppointmentService.Application.DTOs;
using AppointmentService.Application.Interfaces;
using AppointmentService.Application.UseCases.CreateAppointment;




namespace AppointmentService.Application.UsesCases.CompleteAppointmentHandler
{
    public class CompleteAppointmentHandler
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly IAppointmentRepository _appointmentRepository;


        public CompleteAppointmentHandler (IEventPublisher eventPublisher, IAppointmentRepository appointmentRepository)
        {
            _eventPublisher = eventPublisher;
            _appointmentRepository = appointmentRepository;
        }


        public async Task<AppointmentResponse> Handle (Guid appointmentId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync (appointmentId);
            if (appointment is null)
                throw new Exception("Appointment can't be found");


            appointment.Complete();
            await _appointmentRepository.SaveChangesAsync ();

            await _eventPublisher.PublishAsync("appointment.completed", new AppointmentCompletedEvent
            {
                appointmentId = appointment.Id,
                patientId = appointment.patientId,
                doctorId = appointment.doctorId,
                appointmentDate = appointment.appointmentDate

            });

            return CreateAppointmentHandler.MapToResponse(appointment);
        }

    }
}
