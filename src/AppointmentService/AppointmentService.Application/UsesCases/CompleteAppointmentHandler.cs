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


        // Only the doctor of the appointment can complete it
        public async Task<AppointmentResponse> Handle (Guid appointmentId, Guid doctorUserId)
        {
            var appointment = await _appointmentRepository.GetByIdAsync (appointmentId)
                ?? throw new KeyNotFoundException("Appointment not found");

            if (!appointment.IsDoctor(doctorUserId))
                throw new UnauthorizedAccessException("You cannot complete this appointment");

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
