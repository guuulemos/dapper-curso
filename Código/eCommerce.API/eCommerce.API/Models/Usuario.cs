using System;
using System.Collections.Generic;

namespace eCommerce.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Sexo { get; set; }
        public string RG { get; set; }
        public string Cpf { get; set; }
        public string NomeMae { get; set; }
        public string SituacaoCadastro { get; set; }
        public DateTimeOffset DataCadastro { get; set; }

        // Relacionamento um para um
        public Contato Contato { get; set; }

        // Relacionamento um para muitos
        public ICollection<EnderecoEntrega> EnderecosEntrega { get; set; }

        // Relacionamento muitos para muitos (maneira simples pois temos poucos dados)
        public ICollection<Departamento> Departamentos { get; set; }
    }
}
