using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MTG_Emulator.Backend.DB;
using MTG_Emulator.Backend.DB.Models;
using MTG_Emulator.Unity.Db.DTO.AuthenticationDTO;
using Microsoft.EntityFrameworkCore;
using MTG_Emulator.Unity.Db.DTO.PlayerDTO;

namespace MTG_Emulator.Backend.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : MtgController
    {
        private readonly IConfiguration configuration;
        private readonly UserManager<ApiUser> userManager;
        private readonly MTGContext context;

        public AuthenticationController(
            IConfiguration configuration,
            UserManager<ApiUser> userManager,
            MTGContext context)
        {
            this.configuration = configuration;
            this.userManager = userManager;
            this.context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            // Unauthenticated callers can never create admins
            if (dto.Role == Roles.Admin && !User.IsInRole(Roles.Admin))
                return Forbid();

            var user = new ApiUser { UserName = dto.Username, Email = dto.Email };
            var result = await userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            var role = dto.Role == Roles.Admin ? Roles.Admin : Roles.Player;
            await userManager.AddToRoleAsync(user, role);

            // Auto-create the Player profile
            if (role == Roles.Player)
            {
                var player = new Player
                {
                    Username = dto.Username,
                    ApiUserId = user.Id
                };
                context.Players.Add(player);
                await context.SaveChangesAsync();
            }

            return Ok(new { message = "User registered" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var roles = await userManager.GetRolesAsync(user);
            var token = generateJwt(user, roles);
            var resp = new LoginResponseDto
            {
                Username = user.UserName,
                Token = token
            };
            return Ok(resp);
        }

        private string generateJwt(ApiUser user, IList<string> roles)
        {
            var signingKey = configuration["JWT:SigningKey"]
                ?? throw new InvalidOperationException("JWT:SigningKey is not configured.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email!)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: configuration["JWT:Issuer"],
                audience: configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        
        [HttpPut("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            if (dto.NewPassword != dto.ConfirmPassword)
                return BadRequest("Passwords do not match.");

            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(callerId))
                return Unauthorized();

            ApiUser? user;
            if (User.IsInRole(Roles.Admin) && !string.IsNullOrWhiteSpace(dto.TargetUsername))
            {
                user = await userManager.FindByNameAsync(dto.TargetUsername);
            }
            else
            {
                user = await userManager.FindByIdAsync(callerId);
            }

            if (user == null)
                return NotFound();

            if (!IsOwnerOrAdmin(user.Id))
                return Forbid();

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, dto.NewPassword);

            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return NoContent();
        }
        
        [HttpDelete("delete-account")]
        [Authorize]
        public async Task<IActionResult> DeleteAccount([FromQuery] string? targetUsername = null)
        {
            var callerId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            ApiUser? user;
            if (User.IsInRole(Roles.Admin) && !string.IsNullOrWhiteSpace(targetUsername))
            {
                user = await userManager.FindByNameAsync(targetUsername);
            }
            else
            {
                user = await userManager.FindByIdAsync(callerId!);
            }

            if (user == null)
                return NotFound();

            if (!IsOwnerOrAdmin(user.Id))
                return Forbid();

            var player = await context.Players
                .Include(p => p.Decks)
                .FirstOrDefaultAsync(p => p.ApiUserId == user.Id);

            if (player != null)
            {
                context.Players.Remove(player);
                await context.SaveChangesAsync();
            }

            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors.Select(e => e.Description));

            return NoContent();
        }
    }
}

