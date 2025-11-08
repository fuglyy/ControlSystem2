using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ServiceUsers.Models;
using ServiceUsers.DTOs;
using ServiceUsers.Services;
using System;
using System.Text;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;


namespace ServiceUsers.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : ControllerBase{
        private readonly IConfiguration _configuration;

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model, [FromServices] JwtTokenService jwtService)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, error = new { code = "INVALID_INPUT", message = "Некорректные данные входа" } });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { success = false, error = new { code = "USER_NOT_FOUND", message = "Пользователь не найден" } });

            var validPassword = await _userManager.CheckPasswordAsync(user, model.Password);
            if (!validPassword)
                return Unauthorized(new { success = false, error = new { code = "INVALID_PASSWORD", message = "Неверный пароль" } });

            var roles = await _userManager.GetRolesAsync(user);
            var token = jwtService.GenerateToken(user, roles);

            return Ok(new
            {
                success = true,
                data = new
                {
                    token,
                    user = new { user.Id, user.Email, user.Name, roles }
                }
            });
        }
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            user.Name = model.Name;
            user.Email = model.Email;
            user.UserName = model.Email;
            user.UpdatedAt = DateTime.UtcNow;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new
            {
                Message = "Profile updated",
                user.Id,
                user.Email,
                user.Name
            });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.Name)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}