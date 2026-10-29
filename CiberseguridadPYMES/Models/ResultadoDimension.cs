using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class ResultadoDimension
    {
        public int Id { get; set; }

        public int DiagnosticoId { get; set; }

        public Diagnostico? Diagnostico { get; set; }

        public int DimensionId { get; set; }

        public Dimension? Dimension { get; set; }

        public int PreguntasAplicables { get; set; }

        public int PreguntasCumplidas { get; set; }

        public decimal PorcentajeCumplimiento { get; set; }
    }
}