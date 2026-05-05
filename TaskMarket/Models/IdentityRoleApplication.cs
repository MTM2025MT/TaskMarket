namespace TaskMarket.Models
{
    public class IdentityRoleApplication: Microsoft.AspNetCore.Identity.IdentityRole<int>
    {
        string? Description { get; set; }
    }
}
