using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TaskMarket.Models;

namespace TaskMarket.Helpers
{
    public class VerificationOfEmail: IVerificationOfEmail
    {

        private readonly IConfiguration _configuration;
        private readonly UserManager<User> _userManager;
        public VerificationOfEmail(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }
        public string GerateVerificationToken(int userId)
        {
            // Implementation for generating a verification token
            var claims = new[]{
                new Claim("userId", userId.ToString()),
                new Claim("purpose", "email_verification")
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Secret"]));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(5), // short lifetime!
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public async Task SendVerificationEmailAsync(string email,int userId, CancellationToken cancellationToken = default)
        {
            var token = GerateVerificationToken(userId);

            var link = $"https://localhost:7099/api/Accounts/verify-email?token={token}";
            // Implementation for sending verification email
            var message = new MailMessage();
            message.To.Add(email);
            message.Subject = "Verify your email";
            message.Body = $"Click here: {link}";

            var smtp = new SmtpClient("smtp.gmail.com")
            {
                Credentials = new NetworkCredential(_configuration["MyInformation:Email"], _configuration["MyInformation:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message);
        }

        public async Task<ClaimsPrincipal?> ValidateVerificationToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],

                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
                ),

                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out _);

                var purpose = principal.FindFirst("purpose")?.Value;
                if (purpose != "email_verification")
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

    }
}
