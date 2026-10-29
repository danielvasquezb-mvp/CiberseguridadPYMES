using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Evaluacion
    {
        public int Id { get; set; }

        public int EmpresaId { get; set; }

        public Empresa? Empresa { get; set; }

        public int FormularioId { get; set; }

        public Formulario? Formulario { get; set; }

        public DateTime FechaInicio { get; set; } = DateTime.Now;

        public DateTime? FechaFinalizacion { get; set; }

        // En proceso, Finalizada o Cancelada
        public string Estado { get; set; } = "En proceso";

        public ICollection<Respuesta> Respuestas { get; set; }
            = new List<Respuesta>();

        public Diagnostico? Diagnostico { get; set; }
    }
}
