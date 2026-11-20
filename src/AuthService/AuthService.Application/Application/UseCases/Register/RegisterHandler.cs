using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;
using BCrypt.Net;


namespace AuthService.Application.UseCases.Register
{
    public class RegisterHandler

    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;


        public RegisterHandler (IUserRepository userRepository, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _jwtService = jwtService;
        }

        public async Task<AuthResponse> Handle(RegisterRequest request, string role)
        {
            var exists = await _userRepository.ExistsAsync(request.Email);
            if (exists)
            {
                throw new InvalidOperationException("Email already registered");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = User.Create(request.Email, passwordHash, role,  request.Name);

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new AuthResponse
            {
                Token = _jwtService.GenerateToken(user),
                UserId = user.ID,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                ExpiresAt = _jwtService.GetExpirationDate()

            };

        }

    }
}
