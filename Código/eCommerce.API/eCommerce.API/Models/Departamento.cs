using System.Collections.Generic;

namespace eCommerce.API.Models
{
    public class Departamento
    {
        public int Id { get; set; }
        public string Nome { get; set; }

        // Relacionemento muitos para muitos
        public ICollection<Usuario> Usuarios { get; set; }
    }
}
