using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PatientService.Application.DTOs;
using PatientService.Application.UseCases;
using PatientService.Application.UseCases.UploadDocument;
using System.Security.Claims;
using PatientService.Application.Interfaces;

namespace PatientService.API.Controllers
{
    [ApiController]
    [Route("api/patients")]
    public class PatientController : ControllerBase
    {

        private readonly CreatePatientHandler _createPatientHandler;
        private readonly UploadDocumentHandler _uploadDocumentHandler;

        public PatientController(CreatePatientHandler createPatientHandler, UploadDocumentHandler uploadDocumentHandler)
        {
            _createPatientHandler = createPatientHandler;
            _uploadDocumentHandler = uploadDocumentHandler;
        }


        // ============================================================
        // POST /api/patients
        // Crea el perfil de un paciente
        // ============================================================
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                request.userId = Guid.Parse(userIdClaim);
                request.dateOfBirth = DateTime.SpecifyKind(request.dateOfBirth.Date, DateTimeKind.Utc);
                if (userIdClaim is null)
                    return Unauthorized(new { message = "Token invalido" });

                request.userId = Guid.Parse(userIdClaim);
                var response = await _createPatientHandler.Handle(request);
                return CreatedAtAction(nameof(GetProfile), new { }, response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ============================================================
        // GET /api/patients/profile
        // Obtiene el perfil del paciente autenticado
        // ============================================================
        [HttpGet("profile")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is null)
                    return Unauthorized(new { message = "Token inválido" });

                var userId = Guid.Parse(userIdClaim);
                var response = await _createPatientHandler.GetByUserIdAsync(userId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // ============================================================
        // POST /api/patients/documents
        // Sube un documento médico a S3
        // ============================================================
        [HttpPost("documents")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> UploadDocument(
            IFormFile file,
            [FromQuery] string documentType)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value
                               ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is null)
                    return Unauthorized(new { message = "Token inválido" });

                var userId = Guid.Parse(userIdClaim);

                using var stream = file.OpenReadStream();
                var response = await _uploadDocumentHandler.Handle(
                    userId,
                    stream,
                    file.FileName,
                    file.ContentType,
                    documentType);

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET /api/patients/documents
        [HttpGet("documents")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetDocuments([FromServices] IS3Service s3Service)
        {
            try
            {
                var userIdClaim = User.FindFirst("sub")?.Value
                      ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userIdClaim is null)
                    return Unauthorized(new { message = "Token inválido" });

                var userId = Guid.Parse(userIdClaim);
                var documents = await _createPatientHandler.GetDocuments(userId, s3Service);
                return Ok(documents);

            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }



        }
    }
}
