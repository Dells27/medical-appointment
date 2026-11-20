using NotificationService.Application.DTOs;
using NotificationService.Application.Interfaces;
using NotificationService.Domain.Entities;
using static NotificationService.Application.UsesCases.EmailTemplates;

namespace NotificationService.Application.UsesCases;

// Turns appointment events into emails:
//   appointment.created     -> patient (booking received) + doctor (new appointment)
//   appointment.confirmed   -> patient
//   appointment.cancelled   -> the other party
//   appointment.rescheduled -> patient
// Every attempt is logged in the notifications table, including failures.
public class AppointmentNotificationsHandler
{
    private readonly IEmailService _emailService;
    private readonly INotificationRepository _repository;

    public AppointmentNotificationsHandler(IEmailService emailService, INotificationRepository repository)
    {
        _emailService = emailService;
        _repository = repository;
    }

    public async Task HandleCreated(AppointmentCreatedEvent e)
    {
        var when = When(e.appointmentDate, e.appointmentTime);

        await Send("appointment.created", e.PatientEmail,
            "We received your appointment request",
            Layout("Appointment requested", e.patientName,
                "<p>Your appointment has been scheduled. Your doctor will confirm it soon.</p>" +
                Detail("Doctor", Fallback(e.doctorName, "Your doctor")) +
                Detail("When", when)));

        await Send("appointment.created", e.doctorEmail,
            "New appointment booked",
            Layout("New appointment", e.doctorName,
                "<p>A patient booked an appointment with you. Please confirm it from your dashboard.</p>" +
                Detail("Patient", Fallback(e.patientName, "A patient")) +
                Detail("When", when) +
                (string.IsNullOrWhiteSpace(e.reason) ? "" : Detail("Reason", e.reason))));

        await _repository.SaveChangeAsync();
    }

    public async Task HandleConfirmed(AppointmentConfirmedEvent e)
    {
        await Send("appointment.confirmed", e.patientEmail,
            "Your appointment is confirmed",
            Layout("Appointment confirmed", e.patientName,
                "<p>Your doctor confirmed your appointment.</p>" +
                Detail("Doctor", Fallback(e.doctorName, "Your doctor")) +
                Detail("When", When(e.appointmentDate, e.appointmentTime))));

        await _repository.SaveChangeAsync();
    }

    public async Task HandleCancelled(AppointmentCancelledEvent e)
    {
        var when = When(e.appointmentDate, e.appointmentTime);
        var reason = string.IsNullOrWhiteSpace(e.reason) ? "" : Detail("Reason", e.reason);

        if (e.cancelledByRole == "Doctor")
        {
            await Send("appointment.cancelled", e.patientEmail,
                "Your appointment was cancelled",
                Layout("Appointment cancelled", e.patientName,
                    "<p>Your doctor had to cancel your appointment. You can book a new one at any time.</p>" +
                    Detail("Doctor", Fallback(e.doctorName, "Your doctor")) +
                    Detail("When", when) + reason));
        }
        else
        {
            await Send("appointment.cancelled", e.doctorEmail,
                "An appointment was cancelled",
                Layout("Appointment cancelled", e.doctorName,
                    "<p>A patient cancelled their appointment. The time slot is available again.</p>" +
                    Detail("Patient", Fallback(e.patientName, "A patient")) +
                    Detail("When", when) + reason));
        }

        await _repository.SaveChangeAsync();
    }

    public async Task HandleRescheduled(AppointmentRescheduledEvent e)
    {
        await Send("appointment.rescheduled", e.patientEmail,
            "Your appointment was rescheduled",
            Layout("Appointment rescheduled", e.patientName,
                "<p>Your doctor moved your appointment to a new date and time.</p>" +
                Detail("Doctor", Fallback(e.doctorName, "Your doctor")) +
                Detail("Previous", When(e.previousDate, e.previousTime)) +
                Detail("New", When(e.appointmentDate, e.appointmentTime))));

        await _repository.SaveChangeAsync();
    }

    private async Task Send(string eventType, string? recipient, string subject, string html)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            await _repository.AddAsync(Notification.CreateError(eventType, "(none)", subject, "Recipient email is missing"));
            return;
        }

        try
        {
            await _emailService.SendEmailAsync(recipient, subject, html);
            await _repository.AddAsync(Notification.Create(eventType, recipient, subject));
        }
        catch (Exception ex)
        {
            await _repository.AddAsync(Notification.CreateError(eventType, recipient, subject, ex.Message));
        }
    }

    private static string Fallback(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value) ? fallback : value;
}
