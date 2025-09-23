using BackendCore.Models;
using BackendCore.Services;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using QuestPDF.Fluent;
using System;
using System.Threading.Tasks;

namespace BackendCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportQuestController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ReportQuestController(IConfiguration config)
        {
            _config = config;
        }

        [RequestSizeLimit(20 * 1024 * 1024)] // 20 MB
        [HttpPost("descargar-quest")]
        public IActionResult DescargarInforme([FromBody] InformeDtoQuest datos)
        {
            Console.WriteLine("Entró a DescargarInforme");
            try
            {
                if (datos == null)
                    return BadRequest(new { mensaje = "Datos nulos." });

                datos.Cliente = string.IsNullOrWhiteSpace(datos.Cliente) ? "N/A" : datos.Cliente;
                datos.Horas = datos.Horas ?? 0;
                datos.Direccion = datos.Direccion ?? "";
                datos.Equipo = datos.Equipo ?? "";
                datos.Estado = datos.Estado ?? "";
                datos.Trabajos = datos.Trabajos ?? "";
                datos.Observaciones = datos.Observaciones ?? "";
                datos.LubricanteCarter = datos.LubricanteCarter ?? "";
                datos.LubricanteTransmision = datos.LubricanteTransmision ?? "";
                datos.LubricanteCaja = datos.LubricanteCaja ?? "";
                datos.LubricanteDiferencial = datos.LubricanteDiferencial ?? "";
                datos.LubricanteHidraulico = datos.LubricanteHidraulico ?? "";
                datos.FiltroMotor = datos.FiltroMotor ?? "";
                datos.FiltroAire = datos.FiltroAire ?? "";
                datos.FiltroConvertidor = datos.FiltroConvertidor ?? "";
                datos.FiltroHidraulico = datos.FiltroHidraulico ?? "";
                datos.FiltroRespiradores = datos.FiltroRespiradores ?? "";
                datos.Correo = datos.Correo ?? "";
                datos.FirmaBase64 = datos.FirmaBase64 ?? "";

                if (string.IsNullOrWhiteSpace(datos.Fecha))
                    datos.Fecha = DateTime.Now.ToString("yyyy-MM-dd");

                var pdfBytes = GenerarPdfQuest(datos);
                return File(pdfBytes, "application/pdf", "InformeTecnico.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en DescargarInforme: {ex}");
                return BadRequest(new { mensaje = "Error al generar el PDF", error = ex.ToString() });
            }
        }

        [HttpPost("enviar-correo-quest")]
        public async Task<IActionResult> EnviarCorreo([FromBody] EmailRequest request)
        {
            try
            {
                var pdfBytes = GenerarPdfQuest(request.Parametros); // Parametros debe ser InformeDtoQuest

                var mensaje = "Adjunto encontrarás el informe técnico.";
                await EnviarCorreoConAdjunto(
                    request.Destinatario,
                    "Informe Técnico",
                    mensaje,
                    pdfBytes,
                    "InformeTecnico.pdf"
                );

                return Ok(new { mensaje = "Correo enviado correctamente.", datosRecibidos = request });
            }
            catch (Exception ex)
            {
                return BadRequest(new { mensaje = "Error al enviar el correo", error = ex.Message });
            }
        }

        private byte[] GenerarPdfQuest(InformeDtoQuest datos)
        {
            QuestPDF.Settings.EnableDebugging = true;  // Activar modo debugging para detectar conflictos

            Console.WriteLine($"Longitud firma base64 backend: {datos.FirmaBase64?.Length}");
            var documento = new InformeTecnicoQuestPDF(datos);
            return documento.GeneratePdf();
        }

        private async Task EnviarCorreoConAdjunto(string destinatario, string asunto, string mensaje, byte[] adjunto, string nombreAdjunto)
        {
            var emailFrom = _config["EmailSettings:EmailFrom"];
            var appPassword = _config["EmailSettings:AppPassword"];
            var smtpServer = _config["EmailSettings:SmtpServer"];
            var smtpPort = int.Parse(_config["EmailSettings:SmtpPort"]);

            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(emailFrom));
            email.To.Add(MailboxAddress.Parse(destinatario));
            email.Subject = asunto;

            var builder = new BodyBuilder
            {
                TextBody = mensaje
            };

            builder.Attachments.Add(nombreAdjunto, adjunto, new ContentType("application", "pdf"));
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(smtpServer, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(emailFrom, appPassword);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
