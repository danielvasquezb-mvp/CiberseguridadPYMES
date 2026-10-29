using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Dimension
    {
        public int Id { get; set; }

        public string Codigo { get; set; } = string.Empty;

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Pregunta> Preguntas { get; set; }
            = new List<Pregunta>();
    }
}
