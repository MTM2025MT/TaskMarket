#nullable enable
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskMarket.Dtos
{
    public class ContractorRegisterRequestDto
    {
        // Basic Account Credentials (USERS)
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, MinLength(6)]
        public string Password { get; set; } = string.Empty;

        // Professional Profile (CONTRACTORS)
        [Required]
        public string BusinessName { get; set; } = string.Empty;

        public string? Bio { get; set; }

        [Range(0, double.MaxValue)]
        public decimal HourlyRate { get; set; }

        // Skill Specialization (CONTRACTORSKILLS / SERVICECATEGORIES)
        [Required, MinLength(1)]
        public List<int> ContractorServiceCategoriesIds { get; set; } = new();

        // Certificate file paths
        public List<string>? CertificateFilesPaths { get; set; }

        // Platform Verification
        public bool IsVerified { get; set; } = false;
    }
}
