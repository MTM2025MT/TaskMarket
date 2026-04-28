using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos
{
    public class RegisterRequestDto
    {
        [Required]
        public required string FirstName { get; set; }
        [Required]
        public required string LastName { get; set; }

        [Required]
        public required string UserName { get; set; }
        [Required]
        public required string Email { get; set; }
        [Required]
        [MinLength(6)]
        [MaxLength(100)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public required string Password { get; set; }
    }
}
