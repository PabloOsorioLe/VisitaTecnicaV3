using AspNetCore.Reporting;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System.Text;
using System.Text.Json;

namespace BackendCore.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReporteController : ControllerBase
    {
        private readonly IConfiguration _config;

        public ReporteController(IConfiguration config)
        {
            _config = config;
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        //Endpoint para descargar el informe en PDF
        //[HttpPost("descargar")]
        //public IActionResult DescargarInforme([FromBody] InformeDto datos)
        //{
        //    try
        //    {
        //        var pdfBytes = GenerarPdfDesdeDatos(datos);
        //        return File(pdfBytes, "application/pdf", "InformeTecnico.pdf");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(new { mensaje = "Error al generar el reporte", error = ex.Message, stack = ex.StackTrace });
        //    }
        //}

        //[HttpPost("enviar-correo")]
        //public async Task<IActionResult> EnviarCorreo([FromBody] EmailRequest request)
        //{
        //    try
        //    {
        //        var jsonRecibido = JsonSerializer.Serialize(request, new JsonSerializerOptions { WriteIndented = true });
        //        //Console.WriteLine("📥 JSON recibido:\n" + jsonRecibido);
        //        var pdfBytes = GenerarPdfDesdeDatos(request.Parametros);

        //        var mensaje = "Adjunto encontrarás el informe técnico solicitado.";

        //        await EnviarCorreoConAdjunto(request.Destinatario, "Informe Técnico generado", mensaje, pdfBytes, "InformeTecnico.pdf");

        //        // DEVOLVEMOS EL JSON RECIBIDO en la respuesta para depuración
        //        return Ok(new { mensaje = "Correo enviado correctamente.", datosRecibidos = request });
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"Error al enviar el correo: {ex.Message}");
        //    }
        //}


        //Función para generar el PDF como byte[]
        //private byte[] GenerarPdfDesdeDatos(InformeDto datos)
        //{
        //    var rutaReporte = Path.Combine(Directory.GetCurrentDirectory(), "Reports", "RptInformeTecnico.rdlc");
        //    var reporte = new LocalReport(rutaReporte);

        //    var lista = new List<InformeDto> { datos };
        //    reporte.AddDataSource("DsInforme", lista);

        //    var resultado = reporte.Execute(RenderType.Pdf, 1, null, null);
        //    return resultado.MainStream;
        //}

        //Función para enviar el correo con adjunto
        private async Task EnviarCorreoConAdjunto(string destinatario, string asunto, string mensaje, byte[] adjunto, string nombreAdjunto)
        {
            Console.WriteLine("Enviando correo a1: ");
            // Leer desde appsettings.json
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

    //Modelo de datos que se usa tanto para PDF como para correo
    //public class InformeDto
    //{
    //    public string Cliente { get; set; } = "";
    //    public string Fecha { get; set; } = "";
    //    public string Direccion { get; set; } = "";
    //    public string Equipo { get; set; } = "";
    //    public int Horas { get; set; } = 0;
    //    public string Estado { get; set; } = "";
    //    public string Trabajos { get; set; } = "";
    //    public string Observaciones { get; set; } = "";

    //    public string LubricanteCarter { get; set; } = "";
    //    public string LubricanteTransmision { get; set; } = "";
    //    public string LubricanteCaja { get; set; } = "";
    //    public string LubricanteDiferencial { get; set; } = "";
    //    public string LubricanteHidraulico { get; set; } = "";

    //    public string FiltroMotor { get; set; } = "";
    //    public string FiltroAire { get; set; } = "";
    //    public string FiltroConvertidor { get; set; } = "";
    //    public string FiltroHidraulico { get; set; } = "";
    //    public string FiltroRespiradores { get; set; } = "";

    //    public string Correo { get; set; } = "";
    //    public string FirmaBase64 { get; set; } = "";
    //}

    ////Modelo para recibir datos del frontend para envío de correo
    //public class EmailRequest
    //{
    //    public string Destinatario { get; set; } = "";
    //    public InformeDto Parametros { get; set; }
    //}
}
