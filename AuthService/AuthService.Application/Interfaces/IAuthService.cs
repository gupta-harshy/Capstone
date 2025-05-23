using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponse> LoginAsync(LoginRequest request);
        Task<AuthResponse> RegisterAsync(RegisterRequest request);
        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<AuthResponse> GenerateTokenByUserIdAsync(Guid userId); // New method
    }
}