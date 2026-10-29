using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Hallazgo
    {
        public int Id { get; set; }

        public int DiagnosticoId { get; set; }

        public Diagnostico? Diagnostico { get; set; }

        public int PreguntaId { get; set; }

        public Pregunta? Pregunta { get; set; }

        public string Descripcion { get; set; } = string.Empty;

        public string Prioridad { get; set; } = "Media";

        public ICollection<Recomendacion> Recomendaciones { get; set; }
            = new List<Recomendacion>();
    }
}