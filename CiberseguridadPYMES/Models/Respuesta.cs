using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Respuesta
    {
        public int Id { get; set; }

        public int EvaluacionId { get; set; }

        public Evaluacion? Evaluacion { get; set; }

        public int PreguntaId { get; set; }

        public Pregunta? Pregunta { get; set; }

        // 1 = Sí, 2 = Parcialmente, 3 = No,
        // 4 = No sabe / no conoce, 5 = No aplica
        public int Valor { get; set; }

        public string? Observaciones { get; set; }

        public DateTime FechaRegistro { get; set; }
            = DateTime.Now;
    }
}
