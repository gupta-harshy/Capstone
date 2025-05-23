using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Infrastructure.Persistence;
using AuthService.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.AuthService.Infrastructure.Helpers;



namespace AuthService.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AuthDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AuthDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);
            if (user == null) throw new Exception("Invalid credentials");
            return await GenerateTokenAsync(user);
        }

        public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
        {
            var user = new User
            {
                Email = request.Email,
                Password = request.Password,
                Role = request.Role
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return await GenerateTokenAsync(user);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken);
            if (user == null) throw new Exception("Invalid refresh token");
            return await GenerateTokenAsync(user);
        }

        private async Task<AuthResponse> GenerateTokenAsync(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(60);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim("role", user.Role),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("userId", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            user.RefreshToken = TokenHelper.GenerateSecureRefreshToken();
            await _context.SaveChangesAsync();

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                RefreshToken = user.RefreshToken,
                ExpiresAt = expires
            };
        }

        public async Task<AuthResponse> GenerateTokenByUserIdAsync(Guid userId)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new ArgumentException("User not found");
            }

            var response = await GenerateTokenAsync(user);
            return response;
        }
    }
}