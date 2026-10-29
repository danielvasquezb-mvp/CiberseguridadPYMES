using CiberseguridadPYMES.Models;
using Microsoft.AspNetCore.Identity;

namespace CiberseguridadPYMES.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string NombreCompleto { get; set; } = string.Empty;

        public ICollection<Empresa> Empresas { get; set; }
            = new List<Empresa>();
    }
}
