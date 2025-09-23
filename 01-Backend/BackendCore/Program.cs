using AspNetCore.Reporting;
using BackendCore.Data;
using BackendCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text;
using QuestPDF.Drawing;
using QuestPDF.Infrastructure;
using System.IO;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

QuestPDF.Settings.License = LicenseType.Community;

// Registrar fuente personalizada SIN alias
var fontPath = Path.Combine(AppContext.BaseDirectory, "Fonts", "OpenSans-Regular.ttf");

if (!File.Exists(fontPath))
{
    Console.WriteLine($"❌ Fuente NO encontrada: {fontPath}");
}
else
{
    Console.WriteLine($"✅ Fuente encontrada: {fontPath}");
    FontManager.RegisterFont(File.OpenRead(fontPath));
}

var builder = WebApplication.CreateBuilder(args);
// Soporte para páginas de código extendidas (necesario para RDLC)
Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

// Configurar Kestrel para aceptar cuerpos de petición grandes (hasta 20 MB)
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 20 * 1024 * 1024;

    var portEnv = Environment.GetEnvironmentVariable("PORT");
    var port = !string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var p) ? p : 5000;

    serverOptions.ListenAnyIP(port); // Usa puerto dinámico que asigna Render o local
});

// Obtener cadena de conexión desde configuración
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Validar conexión a base de datos
try
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();
        Console.WriteLine("Conexión a base de datos exitosa.");
    }
}
catch (Exception ex)
{
    Console.WriteLine("Error al conectar a base de datos: " + ex.Message);
}

builder.Services.AddDbContext<BackendCoreContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Encoder =
            System.Text.Encodings.Web.JavaScriptEncoder.Create(System.Text.Unicode.UnicodeRanges.All);
    });

// Registrar PasswordHasher para inyección de dependencias
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API VisitaTecnicaV2",
        Version = "v1"
    });
});

string corsPolicyName = "AllowFrontend";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins(
                "http://localhost:4200",
                "https://fullpega.cl",
                "https://www.fullpega.cl",
                "https://visitatecnicav2.onrender.com",
                "https://visita-tecnica-v2-robb.vercel.app"
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Leer clave secreta robusta desde configuración estándar Jwt:Key
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new Exception("Jwt:Key no configurada en configuración");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "defaultIssuer";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "defaultAudience";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

var app = builder.Build();

// Activar swagger solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.UseCors(corsPolicyName);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware para autenticación y autorización
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();  // Mapear controladores

// Mantener solo endpoints auxiliares que no tienes en controlador
app.MapPost("/api/validate-password", async (
    [FromServices] IPasswordHasher<User> passwordHasher,
    [FromBody] ValidatePasswordModel model) =>
{
    var user = new User();
    var result = passwordHasher.VerifyHashedPassword(user, model.PasswordHash, model.Password);

    if (result == PasswordVerificationResult.Success)
    {
        return Results.Ok(new { Valid = true, Message = "Contraseña válida para el hash." });
    }
    else
    {
        return Results.BadRequest(new { Valid = false, Message = "Contraseña inválida para el hash." });
    }
});

app.MapPost("/api/generate-password-hash", (
    [FromServices] IPasswordHasher<User> passwordHasher,
    [FromBody] PasswordModel model) =>
{
    var user = new User();
    var hash = passwordHasher.HashPassword(user, model.Password);
    return Results.Ok(new { PasswordHash = hash });
});

app.Run();

// Modelos para los endpoints auxiliares
public record ValidatePasswordModel(string PasswordHash, string Password);
public record PasswordModel(string Password);
