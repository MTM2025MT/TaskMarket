using System;
using System.Collections.Generic;
//i want to map the contractor to this Dto and the other way around 
namespace TaskMarket.Dtos
{
    public class ContractorProfileDto
    {
        // --- Inherited from User / IdentityUser ---
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        // --- Contractor Specific Properties ---
        public string BusinessName { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal RatingAvg { get; set; }
        public string AboutMe { get; set; } = string.Empty;
        public string? ProfilePicturePath { get; set; }
        public int? CompletedTaskCount { get; set; }
        public bool IsVerified { get; set; }

        // --- Collections ---
        // Using a DTO for the category to avoid exposing the Entity model
        public List<ServiceCategoryDto> Services { get; set; } = new();
        public List<string>? CertificateFilesPaths { get; set; }
    }

  
}
/*
   public enum ProficiencyLevel
    {
        Unknown = 0,
        Beginner = 1,
        Intermediate = 2,
        Advanced = 3,
        Expert = 4
    }

    public class SkillDto
    {
        public string Name { get; set; } = string.Empty;
        public ProficiencyLevel Level { get; set; } = ProficiencyLevel.Unknown;
    }

    public class CertificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Issuer { get; set; }
        public int? YearObtained { get; set; }
    }

    public class LanguageDto
    {
        public string Name { get; set; } = string.Empty;
        public ProficiencyLevel Proficiency { get; set; } = ProficiencyLevel.Unknown;
    }

    public class PortfolioItemDto
    {
        public string Url { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? Description { get; set; }
    }

    public class AddressDto
    {
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
    }

    public class ContactDto
    {
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public Dictionary<string, string> SocialLinks { get; set; } = new(); // e.g. {"linkedin","https://..."}
    }

    public class AvailabilityDto
    {
        public bool IsAvailable { get; set; } = true;
        public string? Note { get; set; }
        public DateTime? NextAvailableDate { get; set; }
    }

    public class ReviewSummaryDto
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int TotalCompletedJobs { get; set; }
    }

    public class ContractorProfileDto
    {
        // Identifiers
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        // Personal
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public string? PhotoUrl { get; set; }

        // Contact & Address
        public ContactDto Contact { get; set; } = new();
        public AddressDto Address { get; set; } = new();

        // Professional details
        public List<SkillDto> Skills { get; set; } = new();
        public List<CertificationDto> Certifications { get; set; } = new();
        public List<LanguageDto> Languages { get; set; } = new();
        public List<PortfolioItemDto> Portfolio { get; set; } = new();
        public List<string> Tags { get; set; } = new();

        // Rates and metrics
        public decimal HourlyRate { get; set; }
        public ReviewSummaryDto ReviewSummary { get; set; } = new();
        public int CompletedJobs => ReviewSummary?.TotalCompletedJobs ?? 0;

        // Availability & audit
        public AvailabilityDto Availability { get; set; } = new();
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }*/
