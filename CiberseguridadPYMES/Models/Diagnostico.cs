using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Diagnostico
    {
        public int Id { get; set; }

        public int EvaluacionId { get; set; }

        public Evaluacion? Evaluacion { get; set; }

        public DateTime FechaGeneracion { get; set; }
            = DateTime.Now;

        public decimal PorcentajeGeneral { get; set; }

        public string Resumen { get; set; } = string.Empty;

        public ICollection<ResultadoDimension> ResultadosDimension { get; set; }
            = new List<ResultadoDimension>();

        public ICollection<Hallazgo> Hallazgos { get; set; }
            = new List<Hallazgo>();
    }
}