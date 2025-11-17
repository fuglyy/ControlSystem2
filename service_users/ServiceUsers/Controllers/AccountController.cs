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
using Microsoft.EntityFrameworkCore;   
using ServiceUsers.Data;               



namespace ServiceUsers.Controllers
{
    [ApiController]
    [Route("api/v1/account")]
    public class AccountController : ControllerBase{
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly JwtTokenService _jwtService;

        public AccountController(ApplicationDbContext dbContext, IConfiguration configuration, UserManager<ApplicationUser> userManager, JwtTokenService jwtService)
        {
            _dbContext = dbContext;
            _configuration = configuration;
            _userManager = userManager;
            _jwtService = jwtService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegistrationRequest model){
            if (!ModelState.IsValid){
                return BadRequest(ModelState);
            }

            var user = new ApplicationUser{
                UserName = model.Email, 
                Email = model.Email, 
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
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, error = new { code = "INVALID_INPUT", message = "Некорректные данные входа" } });

                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user == null)
                    return Unauthorized(new { success = false, error = new { code = "USER_NOT_FOUND", message = "Пользователь не найден" } });

                var validPassword = await _userManager.CheckPasswordAsync(user, model.Password);
                if (!validPassword)
                    return Unauthorized(new { success = false, error = new { code = "INVALID_PASSWORD", message = "Неверный пароль" } });

                var roles = (await _userManager.GetRolesAsync(user)) ?? new List<string>();
                var token = _jwtService.GenerateToken(user, roles);

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
            catch(Exception ex)
            {
                // просто для диагностики
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest model)
        {
            var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var user = await _dbContext.Users.FindAsync(userId);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(model.Email))
            {
                if (await _dbContext.Users.AnyAsync(u => u.Email == model.Email && u.Id != userId))
                    return BadRequest("Email already exists");
                user.Email = model.Email;
            }

            if (!string.IsNullOrEmpty(model.Name)) user.Name = model.Name;
            if (!string.IsNullOrEmpty(model.Password))
            {
                // Используем встроенный хешер из UserManager
                user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, model.Password);
            }

            await _dbContext.SaveChangesAsync();
            return Ok(user);
        }

        [HttpGet]
        [Route("/api/v1/users")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? role = null)
        {
            var query = _dbContext.Users.AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = _dbContext.Users;                        // если Roles хранится через IdentityUserRole
            }

            var totalUsers = await query.CountAsync();
            var users = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new { u.Id, u.Email, u.Name })
                .ToListAsync();

            return Ok(new
            {
                page,
                pageSize,
                totalUsers,
                users
            });
        }

    }
}