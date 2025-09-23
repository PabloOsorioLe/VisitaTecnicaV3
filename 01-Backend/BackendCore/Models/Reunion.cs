using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BackendCore.Models
{
    public class Reunion
    {
        public int Id { get; set; }                    // ID 
        public string Titulo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Glosa { get; set; } = string.Empty;
        public string GlosaDetalle { get; set; } = string.Empty; // nuevo campo
        public string Participante { get; set; } = string.Empty; // nuevo campo
                                                                 // Campo no persistente, solo para mostrar número de fila
        [NotMapped] public int Contador { get; set; }
    }
}
