using System.Security.Cryptography;

namespace AuthService.AuthService.Infrastructure.Helpers
{
    public static class TokenHelper
    {
        public static string GenerateSecureRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}
