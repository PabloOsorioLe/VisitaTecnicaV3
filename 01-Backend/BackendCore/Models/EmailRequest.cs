namespace BackendCore.Models
{
    public class EmailRequest
    {
        public string Destinatario { get; set; } = "";
        public InformeDtoQuest Parametros { get; set; } = new InformeDtoQuest();
    }
}
