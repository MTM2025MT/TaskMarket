using ErrorOr;
using Microsoft.AspNetCore.Http;
using TaskMarket.Models;

namespace TaskMarket.Helpers
{
    public interface IGenerateToken
    {
        string GenerateTheRefreshToken();
        Task<ErrorOr<User>> AddingRefreshTokenToUserAsync(User user, HttpResponse response);
        Task<ErrorOr<string>> GenerateAccessTokenAsync(User user);
    }
}
