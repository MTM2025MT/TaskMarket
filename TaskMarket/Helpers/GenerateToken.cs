using Azure;
using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TaskMarket.Models;

namespace TaskMarket.Helpers
{
    public class GenerateToken : IGenerateToken
    {
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public GenerateToken(UserManager<User> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public string GenerateTheRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private void AppendRefreshTokenCookie(HttpResponse response, string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires,
                Secure = true,
                SameSite = SameSiteMode.None
            };

            response.Cookies.Append("jwt", refreshToken, cookieOptions);
        }

        public async Task<ErrorOr<User>> AddingRefreshTokenToUserAsync(User user, HttpResponse response)
        {
            var refreshToken = GenerateTheRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddHours(8);

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return Error.Failure(code: "refresh-token.update.failed", description: "Failed to update the user with the refresh token.");
            }

            AppendRefreshTokenCookie(response, refreshToken, user.RefreshTokenExpiryTime);

            return user;
        }
                
        public async Task<ErrorOr<string>> GenerateAccessTokenAsync(User user)
        {
            if (user is null)
            {
                return Error.Failure(code: "user.null", description: "User is required.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("account_valid", (user.IsActive && user.IsVerified && !user.IsDeleted).ToString().ToLowerInvariant())
            };

            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _configuration["JWT:Secret"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return Error.Failure(code: "jwt.secret.missing", description: "JWT secret is missing.");
            }

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var issuer = _configuration["JWT:Issuer"];
            var audience = _configuration["JWT:Audience"];

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Issuer = issuer,
                Audience = audience,
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = credentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var accessToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

            return accessToken;
        }
    }
}