using eCommerce.API.Models;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Dapper;
using System;

namespace eCommerce.API.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private IDbConnection _connection;
        public UsuarioRepository()
        {
            _connection = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=eCommerce;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False");
        }

        public List<Usuario> Get()
        {
            List<Usuario> usuarios = new List<Usuario>();

            string sql = "SELECT * FROM Usuarios as U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega EE ON EE.UsuarioId = U.Id";

            // Usamos o query quando queremos retornar algo
            // Relacionamento um para muitos
            _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(sql, (usuario, contato, enderecoEntrega) => {

                // Evita duplicidade
                if (usuarios.SingleOrDefault(a => a.Id == usuario.Id) == null )
                {
                    usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                    usuario.Contato = contato;
                    usuarios.Add(usuario);
                }
                else
                {
                    usuario = usuarios.SingleOrDefault(a => a.Id == usuario.Id);
                }

                usuario.EnderecosEntrega.Add(enderecoEntrega);

                return usuario;
            });

            // Aqui vamos retornar os usuarios com seus contatos e endereços
            return usuarios;
        }

        public Usuario Get(int id)
        {
            List<Usuario> usuarios = new List<Usuario>();

            string sql = "SELECT * FROM Usuarios as U LEFT JOIN Contatos C ON C.UsuarioId = U.Id LEFT JOIN EnderecosEntrega EE ON EE.UsuarioId = U.Id WHERE U.Id = @Id";

            // Usamos o query quando queremos retornar algo
            // Relacionamento um para muitos
            _connection.Query<Usuario, Contato, EnderecoEntrega, Usuario>(sql, (usuario, contato, enderecoEntrega) => {

                // Evita duplicidade
                if (usuarios.SingleOrDefault(a => a.Id == usuario.Id) == null)
                {
                    usuario.EnderecosEntrega = new List<EnderecoEntrega>();
                    usuario.Contato = contato;
                    usuarios.Add(usuario);
                }
                else
                {
                    usuario = usuarios.SingleOrDefault(a => a.Id == usuario.Id);
                }

                usuario.EnderecosEntrega.Add(enderecoEntrega);

                return usuario;
            }, new { Id = id });

            // Aqui vamos retornar os usuarios com seus contatos e endereços
            return usuarios.SingleOrDefault();
        }

        /* public Usuario Get(int id)
        {
            // Relacionamento um para um usando LEFT JOIN
            return _connection.Query<Usuario, Contato, Usuario>("SELECT * FROM USUARIOS as U LEFT JOIN Contatos C ON C.UsuarioId = U.Id WHERE U.Id = @Id",
                (usuario, contato) => 
                {
                    usuario.Contato = contato;
                    return usuario;
                },
                new {Id = id}
            ).FirstOrDefault();
        } */

        public void Insert(Usuario usuario)
        {
            _connection.Open();

            // Caso ocorra algum erro em uma das transações, o transaction vai cancelar tudo
            var transaction = _connection.BeginTransaction();

            try
            {
                string sqlUsuario = "INSERT INTO Usuarios(Nome, Email, Sexo, RG, Cpf, NomeMae, SituacaoCadastro, DataCadastro) VALUES (@Nome, @Email, @Sexo, @RG, @Cpf, @NomeMae, @SituacaoCadastro, @DataCadastro); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                usuario.Id = _connection.Query<int>(sqlUsuario, usuario, transaction).Single();

                if (usuario.Contato != null)
                {
                    usuario.Contato.UsuarioId = usuario.Id;
                    string sqlContato = "INSERT INTO Contatos(UsuarioId, Telefone, Celular) VALUES (@UsuarioId, @Telefone, @Celular); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                    usuario.Contato.Id = _connection.Query<int>(sqlContato, usuario.Contato, transaction).Single();
                }

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    throw new Exception("Ops, parece que algo deu errado! Tente novamente.");
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Update(Usuario usuario)
        {
            _connection.Open();

            var transaction = _connection.BeginTransaction();

            try
            {
                string sqlUsuario = "UPDATE Usuarios SET Nome = @Nome, Email = @Email, Sexo = @Sexo, RG = @RG, Cpf = @Cpf, NomeMae = @NomeMae, SituacaoCadastro = @SituacaoCadastro, DataCadastro = @DataCadastro WHERE Id = @Id";
                _connection.Execute(sqlUsuario, usuario, transaction);

                if (usuario.Contato != null)
                {
                    string sqlContato = "UPDATE Contatos SET UsuarioId = @UsuarioId, Telefone = @Telefone, Celular = @Celular WHERE Id = @Id";
                    _connection.Execute(sqlContato, usuario.Contato, transaction);
                }

                string sqlDeletarEnderecosEntrega = "DELETE FROM EnderecosEntrega WHERE UsuarioId = @Id";
                _connection.Execute(sqlDeletarEnderecosEntrega, usuario, transaction);

                if (usuario.EnderecosEntrega != null && usuario.EnderecosEntrega.Count > 0)
                {
                    foreach (var enderecoEntrega in usuario.EnderecosEntrega)
                    {
                        enderecoEntrega.UsuarioId = usuario.Id;
                        string sqlEndereco = "INSERT INTO EnderecosEntrega(UsuarioId, NomeEndereco, CEP, Estado, Cidade, Bairro, Endereco, Numero, Complemento) VALUES (@UsuarioId, @NomeEndereco, @CEP, @Estado, @Cidade, @Bairro, @Endereco, @Numero, @Complemento); SELECT CAST(SCOPE_IDENTITY() AS INT);";
                        enderecoEntrega.Id = _connection.Query<int>(sqlEndereco, enderecoEntrega, transaction).Single();
                    }
                }

                transaction.Commit();
            }
            catch (Exception)
            {
                try
                {
                    transaction.Rollback();
                }
                catch (Exception)
                {
                    throw new Exception("Ops, parece que algo deu errado! Tente novamente.");
                }
            }
            finally
            {
                _connection.Close();
            }
        }

        public void Delete(int id)
        {
            _connection.Execute("DELETE FROM Usuarios WHERE Id = @Id", new { Id = id });
        }
    }
}
