using Microsoft.AspNetCore.Mvc;
using AuthService.Application.DTOs;
using AuthService.Application.UseCases.Login;
using AuthService.Application.UseCases.Register;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace AuthService.API.Controllers
{

    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        public const string AuthCookieName = "medic_token";

        private readonly RegisterHandler _registerHandler;
        private readonly LoginHandler _loginHandler;
        private readonly IConfiguration _configuration;


        public AuthController(RegisterHandler registerHanlder, LoginHandler loginHandler, IConfiguration configuration)
        {
            _registerHandler = registerHanlder;
            _loginHandler = loginHandler;
            _configuration = configuration;
        }

        // POST /api/auth/register/patient
        [HttpPost("register/patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _registerHandler.Handle(request, "Patient");
                return Ok(SignIn(response));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/auth/register/doctor
        [HttpPost("register/doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterRequest request)
        {
            try
            {
                var response = await _registerHandler.Handle(request, "Doctor");
                return Ok(SignIn(response));
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _loginHandler.Login(request);
                return Ok(SignIn(response));
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        // POST /api/auth/logout
        // Removes the auth cookie. Anonymous on purpose: an expired session must still be able to log out.
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            Response.Cookies.Delete(AuthCookieName, BuildCookieOptions(expires: null));
            return NoContent();
        }

        // ============================================================
        // GET /api/auth/me
        // Returns the authenticated user, read from the JWT claims.
        // The frontend uses it to restore the session on page load.
        // ============================================================
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst("email")?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value ?? User.FindFirst("role")?.Value;
            var name = User.FindFirst("name")?.Value;
            var exp = User.FindFirst("exp")?.Value;

            DateTime? expiresAt = long.TryParse(exp, out var seconds)
                ? DateTimeOffset.FromUnixTimeSeconds(seconds).UtcDateTime
                : null;

            return Ok(new { userId, name, email, role, expiresAt });
        }

        // Stores the JWT in an httpOnly cookie and returns the user data without the token
        private AuthUserResponse SignIn(AuthResponse response)
        {
            Response.Cookies.Append(AuthCookieName, response.Token, BuildCookieOptions(response.ExpiresAt));

            return new AuthUserResponse
            {
                UserId = response.UserId,
                Name = response.Name,
                Email = response.Email,
                Role = response.Role,
                ExpiresAt = response.ExpiresAt
            };
        }

        private CookieOptions BuildCookieOptions(DateTime? expires)
        {
            return new CookieOptions
            {
                HttpOnly = true,
                Secure = _configuration.GetValue("AuthCookie:Secure", true),
                SameSite = SameSiteMode.Strict,
                Path = "/",
                Expires = expires
            };
        }
    }

}
