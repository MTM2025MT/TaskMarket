using System.ComponentModel.DataAnnotations;
namespace PotholeApi.DTO
{
    public class LoginDto 
    {
        public required string Email { get; set; }


        [Required]
        public required string Password { get; set; }

    }
}
