using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NotificationService.Application.UsesCases.HandleAppointmentCreated;

public class HandleAppointmentCreatedHandler
{

    private readonly IEmailService _emailService;
    private readonly INotificationRepository _notificationRepository;

    public HandleAppointmentCreatedHandler(IEmailService emailService, INotificationRepository notificationRepository)
    {
        _emailService = emailService;
        _notificationRepository = notificationRepository;
    }



public async Task Handle(AppointmentCreatedEvent appointmentCreatedEvent)
    {
        var recipientEmail= appointmentCreatedEvent.PatientEmail;

        // Debug temporal — borrar después
        var subject = "Medical appointment confirmation";
        var htmlbody = $@"
<h2>Your medical appointment has been scheduled.</h2>
<p>Date: {appointmentCreatedEvent.appointmentDate:dd/MM/yyyy}</p>
<p>Time: {appointmentCreatedEvent.appointmentTime}</p>
";

        try
        {
            await _emailService.SendEmailAsync(recipientEmail, subject, htmlbody);
            var log = Notification.Create("appointment.created", recipientEmail, subject);
            await _notificationRepository.AddAsync(log);
        }
        catch (Exception ex)
        {
            var log = Notification.CreateError("appointment.created", recipientEmail, subject, ex.Message);
            await _notificationRepository.AddAsync(log);

        }

        await _notificationRepository.SaveChangeAsync();
    }
}
