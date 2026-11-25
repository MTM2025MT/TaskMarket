using System.Security.Claims;

namespace TaskMarket.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        // Returns the authenticated user's id from the JWT, or 0 when it is missing
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : 0;
        }
    }
}
