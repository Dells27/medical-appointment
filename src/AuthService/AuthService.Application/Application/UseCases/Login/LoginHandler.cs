using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using BCrypt.Net;


namespace AuthService.Application.UseCases.Login
{
    public class LoginHandler
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;

        public LoginHandler(IUserRepository userRepository, IJwtService jwtService)
        {

            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> Login(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null)
            {
                throw new UnauthorizedAccessException("Invalid email or password");

            }

            var isValidPassword = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isValidPassword)
            {
                throw new UnauthorizedAccessException("Invalid email or password");
            }


            return new AuthResponse
            {
                Token = _jwtService.GenerateToken(user),
                UserId = user.ID,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = _jwtService.GetExpirationDate(),

            };

        }
    }
}
