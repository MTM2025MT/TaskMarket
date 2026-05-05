using ErrorOr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PotholeApi.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using TaskMarket.Dtos;
using TaskMarket.Helpers;
using TaskMarket.Models;
namespace TaskMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IVerificationOfEmail _verificationOfEmail;
        private readonly ILogger<AccountsController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IGenerateToken _generateToken;
        public AccountsController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IVerificationOfEmail verificationOfEmail, ILogger<AccountsController> logger, IConfiguration configuration, IGenerateToken generateToken)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _verificationOfEmail = verificationOfEmail;
            _logger = logger;
            _configuration = configuration;
            _generateToken = generateToken;
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
                ModelState.AddModelError("Email", "Email is already taken,if you have this email check you email .");
                await _verificationOfEmail.SendVerificationEmailAsync(user_who_has_email.Email, user_who_has_email.Id);
                return BadRequest(ModelState);
            }


            var user = new User
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

            await _userManager.AddToRoleAsync(user, "Customer");

            await _verificationOfEmail.SendVerificationEmailAsync(user.Email, user.Id);
            return Created();
         }
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        {
            // Implementation for verifying the email using the token
            // This would typically involve validating the token, extracting the user ID, and marking the user's email as verified in the database
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

            var userName = request.BusinessName.Trim();

            if (await _userManager.FindByNameAsync(userName) != null)
            {
                ModelState.AddModelError("BusinessName", "Business name is already taken.");
                return BadRequest(ModelState);
            }

            var existingByEmail = await _userManager.FindByEmailAsync(request.Email);
            if (existingByEmail != null)
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                await _verificationOfEmail.SendVerificationEmailAsync(existingByEmail.Email!, existingByEmail.Id);
                return BadRequest(ModelState);
            }

            var contractor = new Contractor
            {
                UserName = userName,
                Email = request.Email,
                FirstName = userName,
                LastName = userName,
                BusinessName = request.BusinessName,
                Bio = request.Bio,
                HourlyRate = request.HourlyRate,
                IsVerified = false,
                CertificateFilesPaths = request.CertificateFilesPaths
            };

            var result = await _userManager.CreateAsync(contractor, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            await _userManager.AddToRoleAsync(contractor, "Contractor");
            await _verificationOfEmail.SendVerificationEmailAsync(contractor.Email!, contractor.Id);

            return Created();
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

            var errorMessage = "";
            // Generate JWT token
            var accessToken = await _generateToken.GenerateAccessTokenAsync(user);

            if (accessToken.IsError)
            {
                errorMessage = string.Join("; ", accessToken.Errors.Select(e => e.Description));
            }
            // Generate and add refresh token to user
            var Adding_Refresh_Token_To_User_ResultBox = await _generateToken.AddingRefreshTokenToUserAsync(user, Response);

            if (Adding_Refresh_Token_To_User_ResultBox.IsError)
            {
                 errorMessage = string.Join("; ", Adding_Refresh_Token_To_User_ResultBox.Errors.Select(e => e.Description));
                
            }

            return Ok(new
            {
                errorMessage = errorMessage,
                token = accessToken,

                id = user.Id                
            });
        }
    }
}
