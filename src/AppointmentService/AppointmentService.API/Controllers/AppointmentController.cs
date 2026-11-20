using AppointmentService.API.Extensions;
using AppointmentService.Application.DTOs;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Application.UsesCases.CancelAppointment;
using AppointmentService.Application.UsesCases.CompleteAppointmentHandler;
using AppointmentService.Application.UsesCases.ConfirmAppointment;
using AppointmentService.Application.UsesCases.DoctorPatients;
using AppointmentService.Application.UsesCases.DoctorStats;
using AppointmentService.Application.UsesCases.GetAppointment;
using AppointmentService.Application.UsesCases.RescheduleAppointment;
using AppointmentService.Application.UsesCases.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.API.Controllers;

// Errors are mapped to HTTP status codes by ApiExceptionHandler
[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly CreateAppointmentHandler _createHandler;
    private readonly CancelAppointmentHandler _cancelHandler;
    private readonly GetAppointmentService _getHandler;
    private readonly CompleteAppointmentHandler _completeHandler;
    private readonly ConfirmAppointmentHandler _confirmHandler;
    private readonly RescheduleAppointmentHandler _rescheduleHandler;
    private readonly DoctorPatientsHandler _patientsHandler;
    private readonly DoctorStatsHandler _statsHandler;
    private readonly SlotAvailabilityService _slots;

    public AppointmentController(
        CreateAppointmentHandler createHandler,
        CancelAppointmentHandler cancelHandler,
        GetAppointmentService getHandler,
        CompleteAppointmentHandler completeHandler,
        ConfirmAppointmentHandler confirmHandler,
        RescheduleAppointmentHandler rescheduleHandler,
        DoctorPatientsHandler patientsHandler,
        DoctorStatsHandler statsHandler,
        SlotAvailabilityService slots)
    {
        _createHandler = createHandler;
        _cancelHandler = cancelHandler;
        _getHandler = getHandler;
        _completeHandler = completeHandler;
        _confirmHandler = confirmHandler;
        _rescheduleHandler = rescheduleHandler;
        _patientsHandler = patientsHandler;
        _statsHandler = statsHandler;
        _slots = slots;
    }

    // ============================================================
    // Booking
    // ============================================================

    // GET /api/appointments/available-slots?doctorId=&date=2026-12-01
    [HttpGet("available-slots")]
    [Authorize]
    public async Task<IActionResult> GetAvailableSlots([FromQuery] Guid doctorId, [FromQuery] DateOnly date)
    {
        var slots = await _slots.GetAvailableSlotsAsync(doctorId, date);
        return Ok(new AvailableSlotsResponse
        {
            doctorId = doctorId,
            date = date,
            slotMinutes = _slots.SlotMinutes,
            slots = slots
        });
    }

    // POST /api/appointments — patients book an appointment
    [HttpPost]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        request.PatientID = User.GetUserId();
        request.PatientEmail = User.GetEmail();
        request.PatientName = User.GetName();
        request.appointmentDate = DateTime.SpecifyKind(request.appointmentDate.Date, DateTimeKind.Utc);

        var response = await _createHandler.Handle(request);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
    }

    // ============================================================
    // Reading
    // ============================================================

    // GET /api/appointments/{id} — only the patient or doctor of the appointment
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _getHandler.HandleById(id, User.GetUserId()));
    }

    // GET /api/appointments/my-appointments — the authenticated patient's appointments
    [HttpGet("my-appointments")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyAppointments()
    {
        return Ok(await _getHandler.HandleByPatient(User.GetUserId()));
    }

    // GET /api/appointments/my-schedule — the authenticated doctor's appointments
    [HttpGet("my-schedule")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMySchedule()
    {
        return Ok(await _getHandler.HandleByDoctor(User.GetUserId()));
    }

    // ============================================================
    // State changes
    // ============================================================

    // PUT /api/appointments/{id}/confirm — the doctor confirms; the patient is notified
    [HttpPut("{id:guid}/confirm")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Confirm(Guid id)
    {
        return Ok(await _confirmHandler.Handle(id, User.GetUserId()));
    }

    // PUT /api/appointments/{id}/reschedule — the doctor moves it; the patient is notified
    [HttpPut("{id:guid}/reschedule")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request)
    {
        return Ok(await _rescheduleHandler.Handle(id, request, User.GetUserId()));
    }

    // PUT /api/appointments/{id}/cancel — the patient or doctor of the appointment
    [HttpPut("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        return Ok(await _cancelHandler.Handle(id, request.reason, User.GetUserId()));
    }

    // PUT /api/appointments/{id}/complete — only the doctor of the appointment
    [HttpPut("{id:guid}/complete")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> Complete(Guid id)
    {
        return Ok(await _completeHandler.Handle(id, User.GetUserId()));
    }

    // ============================================================
    // Doctor: patients and statistics
    // ============================================================

    // GET /api/appointments/my-patients
    [HttpGet("my-patients")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyPatients()
    {
        return Ok(await _patientsHandler.GetMyPatients(User.GetUserId()));
    }

    // GET /api/appointments/my-patients/{patientId}/appointments
    [HttpGet("my-patients/{patientId:guid}/appointments")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetAppointmentsWithPatient(Guid patientId)
    {
        return Ok(await _patientsHandler.GetAppointmentsWithPatient(User.GetUserId(), patientId));
    }

    // GET /api/appointments/relationship/{patientId} — used by other services before
    // showing a patient's clinical data to a doctor
    [HttpGet("relationship/{patientId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> HasRelationship(Guid patientId)
    {
        var hasRelationship = await _patientsHandler.HasRelationship(User.GetUserId(), patientId);
        return Ok(new { hasRelationship });
    }

    // GET /api/appointments/my-stats?from=&to=
    [HttpGet("my-stats")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyStats([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        return Ok(await _statsHandler.Handle(User.GetUserId(), from, to));
    }
}
