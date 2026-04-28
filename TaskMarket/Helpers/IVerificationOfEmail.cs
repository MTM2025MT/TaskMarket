using System.Security.Claims;

namespace TaskMarket.Helpers
{
    public interface IVerificationOfEmail
    {
        public Task SendVerificationEmailAsync(string email,int userId, CancellationToken cancellationToken = default);
        public  Task<ClaimsPrincipal?> ValidateVerificationToken(string token);
    }
}
