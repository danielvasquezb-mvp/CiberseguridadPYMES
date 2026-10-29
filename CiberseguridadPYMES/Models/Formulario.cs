using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Formulario
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Version { get; set; } = "1.0";

        public bool Activo { get; set; } = true;

        public int TipoSistemaId { get; set; }

        public TipoSistema? TipoSistema { get; set; }

        public ICollection<Pregunta> Preguntas { get; set; }
            = new List<Pregunta>();

        public ICollection<Evaluacion> Evaluaciones { get; set; }
            = new List<Evaluacion>();
    }
}
