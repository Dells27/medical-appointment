namespace AuthService.Application.DTOs
{
    // Internal result of login/register. The token never goes in the response body:
    // the controller stores it in an httpOnly cookie.
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role {  get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }

    // What the client receives
    public class AuthUserResponse
    {
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
