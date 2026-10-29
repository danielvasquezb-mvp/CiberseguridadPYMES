using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class Empresa
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Sector { get; set; } = string.Empty;

        public string Municipio { get; set; } = string.Empty;

        public string ApplicationUserId { get; set; } = string.Empty;

        public ApplicationUser? Usuario { get; set; }

        public ICollection<Evaluacion> Evaluaciones { get; set; }
            = new List<Evaluacion>();
    }
}
