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

Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 20 * 1024 * 1024;

    var portEnv = Environment.GetEnvironmentVariable("PORT");
    var port = !string.IsNullOrEmpty(portEnv) && int.TryParse(portEnv, out var p) ? p : 5000;
    Console.WriteLine($"Puerto configurado para Kestrel: {(string.IsNullOrEmpty(portEnv) ? "5000 (default)" : portEnv)}");

    serverOptions.ListenAnyIP(port);
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

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

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API VisitaTecnicaV3",
        Version = "v1"
    });
});

string corsPolicyName = "AllowFrontend";

var corsOrigins = new[]
{
    "http://localhost:4200",
    "https://fullpega.cl",
    "https://www.fullpega.cl",
    "https://visitatecnicav3.onrender.com",
    "https://visita-tecnica-v3.vercel.app"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: corsPolicyName, policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

Console.WriteLine("CORS configurado para orígenes:");
foreach (var origin in corsOrigins)
{
    Console.WriteLine($" - {origin}");
}

var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    Console.WriteLine("❌ Jwt:Key no configurada");
}
else
{
    Console.WriteLine("✅ Jwt:Key configurada correctamente");
}

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

Console.WriteLine($"Aplicación iniciada en entorno: {app.Environment.EnvironmentName}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();

app.Use(async (context, next) =>
{
    Console.WriteLine($"[Request] {context.Request.Method} {context.Request.Path}");
    await next();
});

app.UseCors(corsPolicyName);

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapPost("/api/validate-password", async (
    [FromServices] IPasswordHasher<User> passwordHasher,
    [FromBody] ValidatePasswordModel model) =>
{
    var user = new User();
    var result = passwordHasher.VerifyHashedPassword(user, model.PasswordHash, model.Password);

    if (result == PasswordVerificationResult.Success)
        return Results.Ok(new { Valid = true, Message = "Contraseña válida para el hash." });
    else
        return Results.BadRequest(new { Valid = false, Message = "Contraseña inválida para el hash." });
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

public record ValidatePasswordModel(string PasswordHash, string Password);
public record PasswordModel(string Password);
