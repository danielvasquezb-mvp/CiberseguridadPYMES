using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Recomendacion
    {
        public int Id { get; set; }

        public int PreguntaId { get; set; }

        public Pregunta? Pregunta { get; set; }

        public int? HallazgoId { get; set; }

        public Hallazgo? Hallazgo { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        // Alta, Media o Baja
        public string Prioridad { get; set; } = "Media";

        public bool Activa { get; set; } = true;
    }
}
