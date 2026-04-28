using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
        public AccountsController(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, IVerificationOfEmail verificationOfEmail)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _verificationOfEmail = verificationOfEmail;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if username or email already exists
            if (await _userManager.FindByNameAsync(request.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return BadRequest(ModelState);

            }
            // Check if email already exists
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                ModelState.AddModelError("Email", "Email is already taken.");
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

            return CreatedAtAction(nameof(Register), new { id = user.Id }, new { user.Id, user.UserName, user.Email });
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
        public async Task<IActionResult> RegisterContractor([FromBody] RegisterRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            // Check if username or email already exists
            if (await _userManager.FindByNameAsync(request.UserName) != null)
            {
                ModelState.AddModelError("UserName", "Username is already taken.");
                return BadRequest(ModelState);
            }
            // Check if email already exists
            if (await _userManager.FindByEmailAsync(request.Email) != null)
            {
                ModelState.AddModelError("Email", "Email is already taken.");
                return BadRequest(ModelState);
            }
        }
    }
}
