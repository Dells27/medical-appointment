using MedicalRecordService.Application.DTOs;
using MedicalRecordService.Application.UseCases.FillNotes;
using MedicalRecordService.Application.UseCases.GetMedicalRecords;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicalRecordService.API.Controllers;

[ApiController]
[Route("api/medical-records")]
public class MedicalRecordController : ControllerBase
{
    private readonly FillNotesHandler _fillNotesHandler;
    private readonly GetMedicalRecordsHandler _getMedicalRecordsHandler;

    public MedicalRecordController(
        FillNotesHandler fillNotesHandler,
        GetMedicalRecordsHandler getMedicalRecordsHandler)
    {
        _fillNotesHandler = fillNotesHandler;
        _getMedicalRecordsHandler = getMedicalRecordsHandler;
    }

    // ============================================================
    // PUT /api/medical-records/{id}/notes
    // El médico llena diagnóstico y tratamiento
    // ============================================================
    [HttpPut("{id:guid}/notes")]
    [Authorize(Roles = "Doctor")]
    public async Task<IActionResult> FillNotes(Guid id, [FromBody] FillNotesRequest request)
    {
        try
        {
            var response = await _fillNotesHandler.Handle(id, request);
            return Ok(response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // ============================================================
    // GET /api/medical-records/{id}
    // ============================================================
    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            var record = await _getMedicalRecordsHandler.HandleById(id);
            return Ok(record);
        }
        catch (Exception ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // ============================================================
    // GET /api/medical-records/my-history
    // El paciente ve todo su historial clínico
    // ============================================================
    [HttpGet("my-history")]
    [Authorize(Roles = "Patient")]
    public async Task<IActionResult> GetMyHistory()
    {
        var userIdClaim = User.FindFirst("sub")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null)
            return Unauthorized(new { message = "Token inválido" });

        var patientId = Guid.Parse(userIdClaim);
        var records = await _getMedicalRecordsHandler.HandleByPatient(patientId);
        return Ok(records);
    }
}