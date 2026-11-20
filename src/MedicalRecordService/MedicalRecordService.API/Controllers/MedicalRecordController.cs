using MedicalRecordService.API.Extensions;
using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Application.UseCases.FillNotes;
using MedicalRecordService.Application.UseCases.GetMedicalRecords;
using MedicalRecordService.Application.UseCases.PatientClinicalData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedicalRecordService.API.Controllers;

// Errors are mapped to HTTP status codes by ApiExceptionHandler
[ApiController]
[Route("api/medical-records")]
public class MedicalRecordController : ControllerBase
{
    private readonly FillNotesHandler _fillNotesHandler;
    private readonly GetMedicalRecordsHandler _getMedicalRecordsHandler;
    private readonly PatientClinicalDataHandler _clinicalDataHandler;

    public MedicalRecordController(
        FillNotesHandler fillNotesHandler,
        GetMedicalRecordsHandler getMedicalRecordsHandler,
        PatientClinicalDataHandler clinicalDataHandler)
    {
        _fillNotesHandler = fillNotesHandler;
        _getMedicalRecordsHandler = getMedicalRecordsHandler;
        _clinicalDataHandler = clinicalDataHandler;
    }

    // ============================================================
    // Consultation records
    // ============================================================

    // PUT /api/medical-records/{id}/notes — the attending doctor writes diagnosis and treatment
    [HttpPut("{id:guid}/notes")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> FillNotes(Guid id, [FromBody] FillNotesRequest request)
    {
        return Ok(await _fillNotesHandler.Handle(id, request, User.GetUserId()));
    }

    // GET /api/medical-records/{id} — the patient or the attending doctor
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        return Ok(await _getMedicalRecordsHandler.HandleById(id, User.GetUserId()));
    }

    // GET /api/medical-records/my-history — the patient's consultation history
    [HttpGet("my-history")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyHistory()
    {
        return Ok(await _getMedicalRecordsHandler.HandleByPatient(User.GetUserId()));
    }

    // GET /api/medical-records/my-consultations — consultations the doctor attended
    [HttpGet("my-consultations")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetMyConsultations()
    {
        return Ok(await _clinicalDataHandler.GetDoctorConsultations(User.GetUserId()));
    }

    // GET /api/medical-records/by-appointment/{appointmentId} — the attending doctor
    [HttpGet("by-appointment/{appointmentId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetByAppointment(Guid appointmentId)
    {
        return Ok(await _getMedicalRecordsHandler.HandleByAppointment(appointmentId, User.GetUserId()));
    }

    // GET /api/medical-records/patient/{patientId} — a doctor reviews the patient's history before the visit
    [HttpGet("patient/{patientId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetPatientHistory(Guid patientId)
    {
        return Ok(await _clinicalDataHandler.GetPatientHistoryForDoctor(patientId));
    }

    // ============================================================
    // Medical background — read by the patient, edited only by doctors
    // ============================================================

    // GET /api/medical-records/background/me
    [HttpGet("background/me")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyBackground()
    {
        return Ok(await _clinicalDataHandler.GetMyBackground(User.GetUserId()));
    }

    // GET /api/medical-records/background/{patientId}
    [HttpGet("background/{patientId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> GetBackground(Guid patientId)
    {
        return Ok(await _clinicalDataHandler.GetBackgroundForDoctor(patientId));
    }

    // PUT /api/medical-records/background/{patientId}
    [HttpPut("background/{patientId:guid}")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> UpdateBackground(Guid patientId, [FromBody] UpdateMedicalBackgroundRequest request)
    {
        return Ok(await _clinicalDataHandler.UpdateBackground(User.GetUserId(), patientId, request));
    }
}
