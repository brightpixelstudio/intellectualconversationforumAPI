
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using intellectualconversationAPI.Data;
using intellectualconversationAPI.Models;
using intellectualconversationAPI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace intellectualconversationAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly IConfiguration _configuration;

        public AuthController(AppDbContext context, PasswordService passwordService, IConfiguration configuration)
        {
            _context = context;
            _passwordService = passwordService;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new
                {
                    message = "Username/email and password are required."
                });
            }

            var login = request.Login.Trim();

            // Look up the member using the stored procedure.
            var users = await _context.UserForLogin
                .FromSqlRaw("CALL GetUserForLogin({0})", login)
                .AsNoTracking()
                .ToListAsync();

            var member = users.FirstOrDefault();

            // Use the same error for missing accounts and invalid passwords.
            if (member == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid username/email or password."
                });
            }

            // Reject blocked or suspended accounts.
            if (member.blocked || member.suspended.HasValue)
            {
                return Unauthorized(new
                {
                    message = "Invalid username/email or password."
                });
            }

            // Verify either an existing hash or a legacy plaintext password.
            bool needsRehash;
            var passwordValid = _passwordService.VerifyPassword(request.Password, member.password,out needsRehash);

            if (!passwordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid username/email or password."
                });
            }

            // Upgrade legacy plaintext passwords after successful login.
            if (needsRehash)
            {
                var newHash = _passwordService.HashPassword(request.Password);
                await _context.Database.ExecuteSqlRawAsync("CALL UpdateUserPasswordById({0}, {1})", member.userid, newHash);
            }

            // Update the last login date
            await _context.Database.ExecuteSqlRawAsync("CALL UpdateUserLastLoginById({0})",member.userid);

            // Read JWT settings from User Secrets or environment variables.
            var issuer = _configuration["Jwt:Issuer"]
                ?? throw new InvalidOperationException(
                    "Jwt:Issuer is missing.");

            var audience = _configuration["Jwt:Audience"]
                ?? throw new InvalidOperationException(
                    "Jwt:Audience is missing.");

            var configuredKey = _configuration["Jwt:Key"]
                ?? throw new InvalidOperationException(
                    "Jwt:Key is missing.");

            var keyBytes = Convert.FromBase64String(configuredKey);

            var signingKey = new SymmetricSecurityKey(keyBytes);

            var credentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);

            var now = DateTime.UtcNow;
            var expires = now.AddMinutes(60);

            var claims = new[]
            {
                new Claim(
                    ClaimTypes.NameIdentifier,
                    member.guid),

                new Claim(
                    ClaimTypes.Name,
                    member.username),

                new Claim(
                    "userid",
                    member.userid.ToString())
            };

            var jwt = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                notBefore: now,
                expires: expires,
                signingCredentials: credentials);

            var token = new JwtSecurityTokenHandler()
                .WriteToken(jwt);

            return Ok(new
            {
                token,
                expiresAt = expires,
                userGuid = member.guid,
                username = member.username
            });
        }
    }
}
