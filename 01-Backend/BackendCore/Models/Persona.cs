using System.ComponentModel.DataAnnotations;

namespace BackendCore.Models
{
    public class Persona
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Nombre { get; set; }

        public string Direccion { get; set; }

        public string Rut { get; set; } 

    }
}
