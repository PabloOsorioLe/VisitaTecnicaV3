using BackendCore.Data;
using BackendCore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<AuthController> _logger;

        public AuthController(BackendCoreContext context, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel login)
        {
            _logger.LogInformation("Login attempt received for RUT: {Rut}", login.Rut);

            var normalizedRut = login.Rut?.Trim().ToUpperInvariant() ?? string.Empty;
            var passwordIngresada = login.Password ?? "";

            if (string.IsNullOrWhiteSpace(normalizedRut) || string.IsNullOrWhiteSpace(passwordIngresada))
            {
                _logger.LogWarning("Login failed: empty RUT or password");
                return BadRequest(new { message = "Rut y contraseña son obligatorios." });
            }

            User user;
            try
            {
                user = await _context.Users.FirstOrDefaultAsync(u => u.Rut.Trim().ToUpper() == normalizedRut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error buscando usuario con RUT: {Rut}", normalizedRut);
                return StatusCode(500, new { message = "Error interno al buscar usuario." });
            }

            if (user == null)
            {
                _logger.LogWarning("Usuario no encontrado con RUT: {Rut}", normalizedRut);
                return Unauthorized(new { message = "Usuario no encontrado" });
            }

            if (!VerifyPassword(passwordIngresada, user.PasswordHash, user))
            {
                _logger.LogWarning("Contraseña inválida para usuario con RUT: {Rut}", normalizedRut);
                return Unauthorized(new { message = "Contraseña inválida" });
            }

            List<int> roles;
            try
            {
                roles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.RegID)
                    .Select(ur => ur.RoleId)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo roles para usuario RUT: {Rut}", normalizedRut);
                return StatusCode(500, new { message = "Error interno al obtener roles de usuario." });
            }

            List<string> permissions;
            try
            {
                permissions = await (from rp in _context.RolePermissions
                                     join p in _context.Permissions on rp.PermissionId equals p.RegID
                                     where roles.Contains(rp.RoleId)
                                     select p.PermissionName)
                                     .Distinct()
                                     .ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo permisos para usuario RUT: {Rut}", normalizedRut);
                return StatusCode(500, new { message = "Error interno al obtener permisos del usuario." });
            }

            var claims = new List<SecurityClaim>
            {
                new SecurityClaim(System.Security.Claims.ClaimTypes.Name, user.UserName),
                new SecurityClaim("Rut", user.Rut)
            };

            foreach (var permission in permissions)
            {
                claims.Add(new SecurityClaim("Permission", permission));
            }

            string keyString = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(keyString))
            {
                _logger.LogError("Jwt:Key no configurada en configuración");
                return StatusCode(500, new { message = "Error de configuración de seguridad." });
            }

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

            try
            {
                _context.UserTokens.Add(userToken);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Token registrado en BD para usuario RUT: {Rut}", normalizedRut);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error guardando token en BD para usuario RUT: {Rut}", normalizedRut);
                // No se interrumpe el flujo, se retorna el token aunque no se haya guardado el token
            }

            _logger.LogInformation("Login exitoso para usuario RUT: {Rut}", normalizedRut);
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
