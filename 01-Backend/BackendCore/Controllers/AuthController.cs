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
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DeviceDetectorNET;

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

        [HttpGet("warmup")]
        public async Task<IActionResult> WarmUpUser()
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Rut == "12345678-9");
                return Ok(); // No importa el resultado para el warm-up.
            }
            catch
            {
                // Manejo opcional, pero el objetivo es solo "despertar" la DB/pool.
                return StatusCode(500);
            }
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

            // Obtener User-Agent y parsear dispositivo
            var userAgentString = Request.Headers["User-Agent"].ToString();
            var dd = new DeviceDetector(userAgentString);
            dd.Parse();

            var deviceType = dd.GetDeviceName() ?? "Unknown device";
            var os = dd.GetOs();
            var osName = os?.Match?.Name ?? "Unknown OS";
            var osVersion = os?.Match?.Version ?? "";
            var deviceInfo = $"{deviceType} - {osName} {osVersion}".Trim();

            // Obtener la IP real
            string ipAddress = null;
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                ipAddress = Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',').FirstOrDefault();
            }
            if (string.IsNullOrEmpty(ipAddress))
            {
                ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            }

            _logger.LogInformation("Request Remote IP: {IP}", HttpContext.Connection.RemoteIpAddress?.ToString());
            _logger.LogInformation("X-Forwarded-For header: {XFF}", Request.Headers["X-Forwarded-For"].ToString());
            _logger.LogInformation("IP detectada antes de filtro: {IP}", ipAddress ?? "NULL");

            bool IsPrivateIP(string ip)
            {
                if (IPAddress.TryParse(ip, out var parsedIp))
                {
                    var bytes = parsedIp.GetAddressBytes();
                    switch (bytes[0])
                    {
                        case 10:
                            return true;
                        case 172:
                            return bytes[1] >= 16 && bytes[1] <= 31;
                        case 192:
                            return bytes[1] == 168;
                        default:
                            return false;
                    }
                }
                return false;
            }

            var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

            if (ipAddress == "::1" || ipAddress == "127.0.0.1" || string.IsNullOrEmpty(ipAddress) || IsPrivateIP(ipAddress))
            {
                if (isDevelopment)
                {
                    ipAddress = "181.173.7.175";  // IP pública real para pruebas en desarrollo
                    _logger.LogInformation("IP forzada en desarrollo a: {IP}", ipAddress);
                }
                else
                {
                    ipAddress = null; // No asignar en producción si es local o privada
                }
            }

            // Obtener país solo si la IP es válida
            string country = null;
            if (!string.IsNullOrEmpty(ipAddress))
            {
                try
                {
                    using (var client = new HttpClient())
                    {
                        var apiUrl = $"https://ipapi.co/{ipAddress}/country_name/";
                        var response = await client.GetAsync(apiUrl);
                        if (response.IsSuccessStatusCode)
                        {
                            country = (await response.Content.ReadAsStringAsync())?.Trim();
                            if (string.IsNullOrWhiteSpace(country) || country.ToLower() == "undefined")
                                country = null;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "No se pudo obtener el país para IP: {IP}", ipAddress);
                    country = null;
                }
            }

            // Convertir UTC a hora local de Chile para QueryAt
            DateTime queryAtLocal;
            try
            {
                var chileTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
                queryAtLocal = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, chileTimeZone);
            }
            catch
            {
                queryAtLocal = DateTime.UtcNow; // fallback a UTC si falla
            }

            var userToken = new UserToken
            {
                UserId = user.RegID,
                Token = tokenString,
                DeviceInfo = deviceInfo,
                IPAddress = ipAddress,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = tokenExpiration,
                IsRevoked = false,
                Country = country,
                QueryAt = queryAtLocal,  // Hora local Chile
                SystemId = 3
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
