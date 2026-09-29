using AppointmentService.Application.DTOs;
using AppointmentService.Application.UseCases.CreateAppointment;
using AppointmentService.Application.UsesCases.GetAppointment;
using AppointmentService.Application.UsesCases.CancelAppointment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AppointmentService.API.Controllers;

[ApiController]
[Route("api/appointments")]
public class AppointmentController : ControllerBase
{
    private readonly CreateAppointmentHandler _createAppointmentHandler;
    private readonly CancelAppointmentHandler _cancelAppointmentHandler;
    private readonly GetAppointmentService _getAppointmentsHandler;

    public AppointmentController(
        CreateAppointmentHandler createAppointmentHandler,
        CancelAppointmentHandler cancelAppointmentHandler,
        GetAppointmentService getAppointmentsHandler)
    {
        _createAppointmentHandler = createAppointmentHandler;
        _cancelAppointmentHandler = cancelAppointmentHandler;
        _getAppointmentsHandler = getAppointmentsHandler;
    }

    // ============================================================
    // POST /api/appointments
    // Agenda una cita — solo pacientes
    // ============================================================
    [HttpPost]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> Create([FromBody] CreateAppointmentRequest request)
    {
        try
        {
            var patientId = GetUserId();
            request.PatientID = patientId;
            // Leer el email del token
            request.PatientEmail =
                User.FindFirst(ClaimTypes.Email)?.Value
                ?? string.Empty;
            var response = await _createAppointmentHandler.Handle(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============================================================
    // GET /api/appointments/{id}
    // ============================================================
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var appointment = await _getAppointmentsHandler.HandleById(id);
            return Ok(appointment);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ============================================================
    // GET /api/appointments/my-appointments
    // Citas del paciente autenticado
    // ============================================================
    [HttpGet("my-appointments")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyAppointments()
    {
        var patientId = GetUserId();
        var appointments = await _getAppointmentsHandler.HandleByPatient(patientId);
        return Ok(appointments);
    }

    // ============================================================
    // GET /api/appointments/my-schedule
    // Citas del médico autenticado
    // ============================================================
    [HttpGet("my-schedule")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMySchedule()
    {
        var doctorId = GetUserId();
        var appointments = await _getAppointmentsHandler.HandleByDoctor(doctorId);
        return Ok(appointments);
    }

    // ============================================================
    // PUT /api/appointments/{id}/cancel
    // ============================================================
    [HttpPut("{id:guid}/cancel")]
    [Authorize]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelAppointmentRequest request)
    {
        try
        {
            var response = await _cancelAppointmentHandler.Handle(id, request.reason);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============================================================
    // Helper — lee el UserId del token JWT
    // ============================================================
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null)
            throw new Exception("Token inválido");

        return Guid.Parse(userIdClaim);
    }
}