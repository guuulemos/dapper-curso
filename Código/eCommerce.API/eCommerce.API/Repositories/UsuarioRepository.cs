﻿using eCommerce.API.Models;
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
            // Usamos o query quando queremos retornar algo
            return _connection.Query<Usuario>("SELECT * FROM Usuarios").ToList();
        }

        public Usuario Get(int id)
        {
            // Relacionamento um para um usando LEFT JOIN
            return _connection.Query<Usuario, Contato, Usuario>("SELECT * FROM USUARIOS AS U LEFT JOIN Contatos C ON C.UsuarioId = U.Id WHERE U.Id = @Id",
                (usuario, contato) => 
                {
                    usuario.Contato = contato;
                    return usuario;
                },
                new {Id = id}
            ).FirstOrDefault();
        }

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
