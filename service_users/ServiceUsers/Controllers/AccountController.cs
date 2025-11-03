using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ServiceUsers.Models;
using ServiceUsers.DTOs;

namespace ServiceUsers.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : ControllerBase{
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(UserManager<ApplicationUser> userManager){
            _userManager = userManager;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest model){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser{
                UserName = model.Email, 
                Name = model.Name,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded){
                return Ok(new 
                {
                    Message = "User registered successfully.", 
                    UserId = user.Id, 
                    Email = user.Email 
                });
            }

            return BadRequest(new 
            {
                Message = "Registration faild",
                Errors = result.Errors.Select(e => e.Description)
            });
        }
    }
}