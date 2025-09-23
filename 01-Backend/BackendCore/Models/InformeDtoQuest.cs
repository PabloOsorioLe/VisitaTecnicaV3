using System.Text.Json.Serialization;

namespace BackendCore.Models
{
    public class InformeDtoQuest
    {
        public string Cliente { get; set; } = "";
        public string Fecha { get; set; } = "";
        public string Direccion { get; set; } = "";
        public string Equipo { get; set; } = "";
        public int? Horas { get; set; }
        public string Estado { get; set; } = "";
        public string Trabajos { get; set; } = "";
        public string Observaciones { get; set; } = "";
        public string LubricanteCarter { get; set; } = "";
        public string LubricanteTransmision { get; set; } = "";
        public string LubricanteCaja { get; set; } = "";
        public string LubricanteDiferencial { get; set; } = "";
        public string LubricanteHidraulico { get; set; } = "";
        public string FiltroMotor { get; set; } = "";
        public string FiltroAire { get; set; } = "";
        public string FiltroConvertidor { get; set; } = "";
        public string FiltroHidraulico { get; set; } = "";
        public string FiltroRespiradores { get; set; } = "";
        public string Correo { get; set; } = "";

        [JsonPropertyName("firmaBase64")]
        public string FirmaBase64 { get; set; }

        // Agregar UserName para mostrar nombre del usuario logeado
        public string? UserName { get; set; }
    }
}
