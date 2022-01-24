using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;

namespace eCommerce.API.Models
{
    [Table("Usuarios")]
    public class UsuarioTwo
    {
        [Key]
        public int Codigo { get; set; }
        public string NomeCompleto { get; set; }
        public string Email { get; set; }
        public string Sexo { get; set; }
        public string RG { get; set; }
        public string Cpf { get; set; }
        public string NomeCompletoMae { get; set; }
        public string Situacao { get; set; }
        public DateTimeOffset DataCadastro { get; set; }

        // Relacionamento um para um
        [Write(false)]
        public Contato Contato { get; set; }

        // Relacionamento um para muitos
        [Write(false)]
        public ICollection<EnderecoEntrega> EnderecosEntrega { get; set; }

        // Relacionamento muitos para muitos (maneira simples pois temos poucos dados)
        [Write(false)]
        public ICollection<Departamento> Departamentos { get; set; }
    }
}
