using CiberseguridadPYMES.Models;

namespace CiberseguridadPYMES.Models
{
    public class TipoSistema
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Descripcion { get; set; } = string.Empty;

        public ICollection<Formulario> Formularios { get; set; }
            = new List<Formulario>();
    }
}
