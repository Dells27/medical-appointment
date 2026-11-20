using DoctorService.API.Extensions;
using DoctorService.Application.DTOs;
using DoctorService.Application.UseCases.CreateDoctor;
using DoctorService.Application.UseCases.GetDoctor;
using DoctorService.Application.UseCases.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoctorService.API.Controllers
{
    // Errors are mapped to HTTP status codes by ApiExceptionHandler
    [ApiController]
    [Route("api/doctors")]
    public class DoctorController : ControllerBase
    {
        private readonly CreateDoctorHanlder _createDoctorHandler;
        private readonly GetDoctorHandler _getDoctorHandler;
        private readonly DoctorProfileHandler _profileHandler;

        public DoctorController(
            CreateDoctorHanlder createDoctorHanlder,
            GetDoctorHandler getDoctorHandler,
            DoctorProfileHandler profileHandler)
        {
            _createDoctorHandler = createDoctorHanlder;
            _getDoctorHandler = getDoctorHandler;
            _profileHandler = profileHandler;
        }

        // ============================================================
        // Public endpoints — anonymous visitors (landing, doctors, specialties)
        // ============================================================

        // GET /api/doctors/public?specialty=&search=
        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublic([FromQuery] string? specialty, [FromQuery] string? search)
        {
            return Ok(await _getDoctorHandler.HandlePublicSearch(specialty, search));
        }

        // GET /api/doctors/public/specialties
        [HttpGet("public/specialties")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSpecialties()
        {
            return Ok(await _getDoctorHandler.HandleGetSpecialties());
        }

        // GET /api/doctors/public/{userId}
        [HttpGet("public/{userId:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicByUserId(Guid userId)
        {
            return Ok(await _getDoctorHandler.HandlePublicGetByUserId(userId));
        }

        // ============================================================
        // Authenticated endpoints
        // ============================================================

        // POST /api/doctors — the authenticated doctor creates their profile
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Create([FromBody] CreateDoctorRequest request)
        {
            request.userId = User.GetUserId();
            var response = await _createDoctorHandler.Handle(request);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }

        // GET /api/doctors?specialty=&search=
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetAll([FromQuery] string? specialty, [FromQuery] string? search)
        {
            return Ok(await _getDoctorHandler.HandleSearch(specialty, search));
        }

        // GET /api/doctors/{id}
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Ok(await _getDoctorHandler.HandleGetById(id));
        }

        // GET /api/doctors/by-user/{userId} — used by other services and the UI
        [HttpGet("by-user/{userId:guid}")]
        [Authorize]
        public async Task<IActionResult> GetByUserId(Guid userId)
        {
            return Ok(await _getDoctorHandler.HandleGetByUserId(userId));
        }

        // GET /api/doctors/by-user/{userId}/availability?date=2026-12-01
        [HttpGet("by-user/{userId:guid}/availability")]
        [Authorize]
        public async Task<IActionResult> GetAvailability(Guid userId, [FromQuery] DateOnly date)
        {
            return Ok(await _profileHandler.GetAvailability(userId, date));
        }

        // GET /api/doctors/specialty/{specialty}
        [HttpGet("specialty/{specialty}")]
        [Authorize]
        public async Task<IActionResult> GetBySpecialty(string specialty)
        {
            return Ok(await _getDoctorHandler.HandleGetBySpecialty(specialty));
        }

        // ============================================================
        // The authenticated doctor's own profile, hours and time off
        // ============================================================

        // GET /api/doctors/me
        [HttpGet("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetMine()
        {
            return Ok(await _profileHandler.GetMine(User.GetUserId()));
        }

        // PUT /api/doctors/me
        [HttpPut("me")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> UpdateMine([FromBody] UpdateDoctorRequest request)
        {
            return Ok(await _profileHandler.UpdateMine(User.GetUserId(), request));
        }

        // PUT /api/doctors/me/schedules — replaces the weekly working hours
        [HttpPut("me/schedules")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> ReplaceSchedules([FromBody] ReplaceSchedulesRequest request)
        {
            return Ok(await _profileHandler.ReplaceSchedules(User.GetUserId(), request));
        }

        // GET /api/doctors/me/time-off
        [HttpGet("me/time-off")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetTimeOff()
        {
            return Ok(await _profileHandler.GetTimeOff(User.GetUserId()));
        }

        // POST /api/doctors/me/time-off — vacation, day off or blocked hours
        [HttpPost("me/time-off")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> AddTimeOff([FromBody] CreateTimeOffRequest request)
        {
            return Ok(await _profileHandler.AddTimeOff(User.GetUserId(), request));
        }

        // DELETE /api/doctors/me/time-off/{id}
        [HttpDelete("me/time-off/{id:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> RemoveTimeOff(Guid id)
        {
            await _profileHandler.RemoveTimeOff(User.GetUserId(), id);
            return NoContent();
        }
    }
}
