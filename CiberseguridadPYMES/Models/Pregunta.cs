
using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Pregunta
    {
        public int Id { get; set; }

        public int FormularioId { get; set; }

        public Formulario? Formulario { get; set; }

        public int DimensionId { get; set; }

        public Dimension? Dimension { get; set; }

        public string Texto { get; set; } = string.Empty;

        public string? ReferenciaTecnica { get; set; }

        public string? CriterioCumplimiento { get; set; }

        public bool Activa { get; set; } = true;

        public int Orden { get; set; }
        public string TipoRespuesta { get; set; } = "CincoOpciones";

        public ICollection<Respuesta> Respuestas { get; set; }
            = new List<Respuesta>();

        public ICollection<Recomendacion> Recomendaciones { get; set; }
            = new List<Recomendacion>();
    }
}
