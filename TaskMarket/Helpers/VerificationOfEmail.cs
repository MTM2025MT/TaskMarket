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

        // Signed with the same key as access tokens; the "purpose" claim keeps the two from being interchangeable
        private SymmetricSecurityKey SigningKey =>
            new(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]
                ?? throw new InvalidOperationException("JWT:Secret is not configured.")));

        public string GerateVerificationToken(int userId)
        {
            var claims = new[]{
                new Claim("userId", userId.ToString()),
                new Claim("purpose", "email_verification")
            };

            var creds = new SigningCredentials(SigningKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task SendVerificationEmailAsync(string email,int userId, CancellationToken cancellationToken = default)
        {
            var token = GerateVerificationToken(userId);

            var baseUrl = (_configuration["App:BaseUrl"] ?? "https://localhost:7264").TrimEnd('/');
            var link = $"{baseUrl}/api/Accounts/verify-email?token={Uri.EscapeDataString(token)}";

            var senderEmail = _configuration["MyInformation:Email"];
            if (string.IsNullOrWhiteSpace(senderEmail))
            {
                throw new InvalidOperationException("MyInformation:Email is not configured.");
            }

            // SmtpClient refuses to send without a From address
            using var message = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = "Verify your email",
                Body = $"Click here: {link}"
            };
            message.To.Add(email);

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(senderEmail, _configuration["MyInformation:Password"]),
                EnableSsl = true
            };

            await smtp.SendMailAsync(message, cancellationToken);
        }

        public Task<ClaimsPrincipal?> ValidateVerificationToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParams = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _configuration["JWT:Issuer"],

                ValidateAudience = true,
                ValidAudience = _configuration["JWT:Audience"],

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey = SigningKey,

                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParams, out _);

                var purpose = principal.FindFirst("purpose")?.Value;
                if (purpose != "email_verification")
                    return Task.FromResult<ClaimsPrincipal?>(null);

                return Task.FromResult<ClaimsPrincipal?>(principal);
            }
            catch
            {
                return Task.FromResult<ClaimsPrincipal?>(null);
            }
        }

    }
}
