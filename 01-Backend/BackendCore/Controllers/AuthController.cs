using BackendCore.Data;
using BackendCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

using SecurityClaim = System.Security.Claims.Claim;

namespace BackendCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly BackendCoreContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher<User> _passwordHasher = new PasswordHasher<User>();

        public AuthController(BackendCoreContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            var normalizedRut = login.Rut.Trim().ToUpperInvariant();
            var passwordIngresada = login.Password ?? "";

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Rut.Trim().ToUpper() == normalizedRut);

            if (user == null)
            {
                return Unauthorized("Usuario no encontrado");
            }

            if (!VerifyPassword(passwordIngresada, user.PasswordHash, user))
            {
                return Unauthorized("Contraseña inválida");
            }

            var roles = await _context.UserRoles
                .Where(ur => ur.UserId == user.RegID)
                .Select(ur => ur.RoleId)
                .ToListAsync();

            var permissions = await (from rp in _context.RolePermissions
                                     join p in _context.Permissions on rp.PermissionId equals p.RegID
                                     where roles.Contains(rp.RoleId)
                                     select p.PermissionName)
                                     .Distinct()
                                     .ToListAsync();

            var claims = new List<SecurityClaim>
            {
                new SecurityClaim(System.Security.Claims.ClaimTypes.Name, user.UserName),
                new SecurityClaim("Rut", user.Rut)
            };

            foreach (var permission in permissions)
            {
                claims.Add(new SecurityClaim("Permission", permission));
            }

            var keyString = _configuration["Jwt:Key"] ?? throw new Exception("Jwt:Key no configurada");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyString));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenExpiration = DateTime.UtcNow.AddHours(1);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: tokenExpiration,
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // Obtener datos del dispositivo y IP de HttpContext
            var deviceInfo = Request.Headers["User-Agent"].ToString();
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Registrar token en la base de datos
            var userToken = new UserToken
            {
                UserId = user.RegID,
                Token = tokenString,
                DeviceInfo = deviceInfo,
                IPAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = tokenExpiration,
                IsRevoked = false
            };

            _context.UserTokens.Add(userToken);
            await _context.SaveChangesAsync();

            return Ok(new { Token = tokenString });
        }

        private bool VerifyPassword(string password, string hashedPassword, User user)
        {
            var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
            return result == PasswordVerificationResult.Success;
        }
    }

    public class LoginModel
    {
        public string Rut { get; set; }
        public string Password { get; set; }
    }
}
