using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PotholeApi.DTO;
using TaskMarket.Dtos;
using TaskMarket.Helpers;
using TaskMarket.IRepositories;
using TaskMarket.Models;
namespace TaskMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IVerificationOfEmail _verificationOfEmail;
        private readonly ILogger<AccountsController> _logger;
        private readonly IGenerateToken _generateToken;
        private readonly IServiceCategoryRepository _serviceCategoryRepository;
        private readonly IContractorRepository _contractorRepository;
        public AccountsController(
            UserManager<User> userManager,
            IVerificationOfEmail verificationOfEmail,
            ILogger<AccountsController> logger,
            IGenerateToken generateToken,
            IServiceCategoryRepository serviceCategoryRepository,
            IContractorRepository contractorRepository)
        {
            _userManager = userManager;
            _verificationOfEmail = verificationOfEmail;
            _logger = logger;
            _generateToken = generateToken;
            _serviceCategoryRepository = serviceCategoryRepository;
            _contractorRepository = contractorRepository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if username or email already exists
            var user_who_has_username = await _userManager.FindByNameAsync(request.UserName);
            if (user_who_has_username != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return BadRequest(ModelState);

            }
            // Check if email already exists
            var user_who_has_email= await _userManager.FindByEmailAsync(request.Email);
            if (user_who_has_email != null)
            {
                ModelState.AddModelError("Email", "Email is already taken, if you have this email check your email.");
                await ResendVerificationIfNeededAsync(user_who_has_email);
                return BadRequest(ModelState);
            }

            // Customers get their own row in the Customers table so they can own tasks, offers and payments
            var user = new Customer
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
            };
            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            await TrySendVerificationEmailAsync(user);
            return StatusCode(StatusCodes.Status201Created, new { id = user.Id });
         }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            var principal = await _verificationOfEmail.ValidateVerificationToken(token);
            if (principal == null)
            {
                return BadRequest("Invalid token.");
            }

            var userId = principal.FindFirst("userId")?.Value;
            if (userId == null)
            {
                return BadRequest("Invalid token.");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest("Invalid token.");
            }

            user.IsVerified = true;
            user.EmailConfirmed = true;
            await _userManager.UpdateAsync(user);

            return Ok("Email verified successfully.");
        }

        [HttpPost("register-contractor")]
        public async Task<IActionResult> RegisterContractor([FromBody] ContractorRegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var businessName = request.BusinessName.Trim();

            if (await _contractorRepository.BusinessNameExistsAsync(businessName))
            {
                ModelState.AddModelError("BusinessName", "Business name is already taken.");
                return BadRequest(ModelState);
            }

            var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingByEmail != null)
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                await ResendVerificationIfNeededAsync(existingByEmail);
                return BadRequest(ModelState);
            }

            var requestedCategoryIds = request.ContractorServiceCategoriesIds.Distinct().ToList();
            var categories = await _serviceCategoryRepository.GetByIdsAsync(requestedCategoryIds);
            if (categories.Count != requestedCategoryIds.Count)
            {
                ModelState.AddModelError("ContractorServiceCategoriesIds", "One or more service categories do not exist.");
                return BadRequest(ModelState);
            }

            var contractor = new Contractor
            {
                // Identity usernames only allow letters, digits and -._@+, so business names with spaces can't be used here
                UserName = request.Email,
                Email = request.Email,
                FirstName = businessName,
                LastName = businessName,
                BusinessName = businessName,
                Bio = request.Bio,
                HourlyRate = request.HourlyRate,
                // Email verification flips this; clients must not be able to self-verify
                IsVerified = false,
                CertificateFilesPaths = request.CertificateFilesPaths,
                ContractorServiceCategories = categories
            };

            var result = await _userManager.CreateAsync(contractor, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(contractor, "Contractor");
            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            await TrySendVerificationEmailAsync(contractor);

            return StatusCode(StatusCodes.Status201Created, new { id = contractor.Id });
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Find the user by email
            User? user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user is  null)
            {
                return Unauthorized("Email or Password are wrong");
            }

            // Check if the password is correct
            if ( !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            {
                return Unauthorized("Email or Password are wrong");
            }

            // Tokens of unverified/inactive accounts are rejected by every protected endpoint, so stop here
            if (!user.IsVerified)
            {
                await ResendVerificationIfNeededAsync(user);
                return Problem(
                    detail: "Please verify your email before logging in. A new verification link was sent.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            if (!user.IsActive || user.IsDeleted)
            {
                return Problem(detail: "This account is disabled.", statusCode: StatusCodes.Status403Forbidden);
            }

            // Generate JWT token
            var accessToken = await _generateToken.GenerateAccessTokenAsync(user);
            if (accessToken.IsError)
            {
                return Problem(
                    detail: string.Join("; ", accessToken.Errors.Select(e => e.Description)),
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            // Generate and add refresh token to user (this also saves LastLogin)
            user.LastLogin = DateTime.UtcNow;
            var Adding_Refresh_Token_To_User_ResultBox = await _generateToken.AddingRefreshTokenToUserAsync(user, Response);

            var errorMessage = "";
            if (Adding_Refresh_Token_To_User_ResultBox.IsError)
            {
                 errorMessage = string.Join("; ", Adding_Refresh_Token_To_User_ResultBox.Errors.Select(e => e.Description));
            }
            var role = await _userManager.GetRolesAsync(user);
            return Ok(new
            {
                errorMessage = errorMessage,
                token = accessToken.Value,
                roles= role,
                id = user.Id
            });
        }

        private async Task ResendVerificationIfNeededAsync(User user)
        {
            if (!user.IsVerified)
            {
                await TrySendVerificationEmailAsync(user);
            }
        }

        // The account is already created at this point, so a mail failure must not turn into a 500
        private async Task TrySendVerificationEmailAsync(User user)
        {
            try
            {
                await _verificationOfEmail.SendVerificationEmailAsync(user.Email!, user.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send verification email to user {UserId}", user.Id);
            }
        }
    }
}
