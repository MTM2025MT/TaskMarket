using TaskMarket.Dtos;
using TaskMarket.Models;

namespace TaskMarket.Factories
{
    public static class ContractorProfileFactory
    {
        // Public profile only: never expose Identity fields such as PasswordHash or RefreshToken
        public static ContractorProfileDto Create(Contractor contractor)
        {
            return new ContractorProfileDto
            {
                Id = contractor.Id,
                FirstName = contractor.FirstName,
                LastName = contractor.LastName,
                Email = contractor.Email,
                PhoneNumber = contractor.PhoneNumber,
                BusinessName = contractor.BusinessName,
                Bio = contractor.Bio,
                HourlyRate = contractor.HourlyRate,
                RatingAvg = contractor.RatingAvg,
                AboutMe = contractor.AboutMe ?? string.Empty,
                ProfilePicturePath = contractor.ProfilePicturePath,
                CompletedTaskCount = contractor.CompletedTaskCount,
                IsVerified = contractor.IsVerified,
                CertificateFilesPaths = contractor.CertificateFilesPaths,
                Services = contractor.ContractorServiceCategories
                    .Select(c => new ServiceCategoryDto { Id = c.Id, Name = c.Name, Slug = c.Slug })
                    .ToList()
            };
        }
    }
}
