using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PatientService.API.Extensions;
using PatientService.Application.DTOs;
using PatientService.Application.Interfaces;
using PatientService.Application.UseCases;
using PatientService.Application.UseCases.UploadDocument;

namespace PatientService.API.Controllers
{
    // Errors are mapped to HTTP status codes by ApiExceptionHandler
    [ApiController]
    [Route("api/patients")]
    public class PatientController : ControllerBase
    {
        // Slightly above the 10 MB file limit to leave room for the multipart envelope
        private const long MaxRequestBytes = 11 * 1024 * 1024;

        private readonly CreatePatientHandler _patientHandler;
        private readonly UploadDocumentHandler _documentHandler;
        private readonly IAppointmentServiceClient _appointments;

        public PatientController(
            CreatePatientHandler patientHandler,
            UploadDocumentHandler documentHandler,
            IAppointmentServiceClient appointments)
        {
            _patientHandler = patientHandler;
            _documentHandler = documentHandler;
            _appointments = appointments;
        }

        // ============================================================
        // The authenticated patient
        // ============================================================

        // POST /api/patients — creates the patient's profile
        [HttpPost]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Create([FromBody] CreatePatientRequest request)
        {
            request.userId = User.GetUserId();
            request.dateOfBirth = DateTime.SpecifyKind(request.dateOfBirth.Date, DateTimeKind.Utc);

            var response = await _patientHandler.Handle(request);
            return CreatedAtAction(nameof(GetProfile), new { }, response);
        }

        // GET /api/patients/profile
        [HttpGet("profile")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetProfile()
        {
            return Ok(await _patientHandler.GetByUserIdAsync(User.GetUserId()));
        }

        // PUT /api/patients/profile — the patient edits their personal information
        [HttpPut("profile")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdatePatientRequest request)
        {
            return Ok(await _patientHandler.UpdateAsync(User.GetUserId(), request));
        }

        // GET /api/patients/documents — own documents plus the ones issued by doctors
        [HttpGet("documents")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetMyDocuments()
        {
            return Ok(await _documentHandler.GetForPatient(User.GetUserId()));
        }

        // POST /api/patients/documents?documentType=LabResult — the patient shares a document
        [HttpPost("documents")]
        [Authorize(Roles = "Patient")]
        [RequestSizeLimit(MaxRequestBytes)]
        public async Task<IActionResult> UploadMyDocument(IFormFile file, [FromQuery] string documentType,
            [FromQuery] string? description)
        {
            var userId = User.GetUserId();
            return Ok(await Upload(userId, new DocumentUploader(userId, "Patient", User.GetName()),
                file, documentType, null, description));
        }

        // ============================================================
        // A doctor working with one of their patients
        // ============================================================

        // GET /api/patients/{patientUserId} — profile summary
        [HttpGet("{patientUserId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetPatientForDoctor(Guid patientUserId)
        {
            return Ok(await _patientHandler.GetForDoctor(patientUserId, _appointments));
        }

        // GET /api/patients/{patientUserId}/documents
        [HttpGet("{patientUserId:guid}/documents")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetPatientDocuments(Guid patientUserId)
        {
            return Ok(await _documentHandler.GetForDoctor(patientUserId));
        }

        // POST /api/patients/{patientUserId}/documents?documentType=Prescription&appointmentId=
        // The doctor uploads a prescription, order, report or certificate
        [HttpPost("{patientUserId:guid}/documents")]
        [Authorize(Roles = "Doctor")]
        [RequestSizeLimit(MaxRequestBytes)]
        public async Task<IActionResult> UploadPatientDocument(Guid patientUserId, IFormFile file,
            [FromQuery] string documentType, [FromQuery] Guid? appointmentId, [FromQuery] string? description)
        {
            var uploader = new DocumentUploader(User.GetUserId(), "Doctor", User.GetName());
            return Ok(await Upload(patientUserId, uploader, file, documentType, appointmentId, description));
        }

        private async Task<MedicalDocumentResponse> Upload(Guid patientUserId, DocumentUploader uploader,
            IFormFile file, string documentType, Guid? appointmentId, string? description)
        {
            if (file is null)
                throw new ArgumentException("A file is required");

            await using var stream = file.OpenReadStream();
            return await _documentHandler.Upload(patientUserId, uploader, new UploadDocumentCommand
            {
                FileStream = stream,
                FileName = Path.GetFileName(file.FileName),
                ContentType = file.ContentType,
                FileSize = file.Length,
                DocumentType = documentType,
                AppointmentId = appointmentId,
                Description = description
            });
        }
    }
}
