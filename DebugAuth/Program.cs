using System;
using Npgsql;

namespace DebugAuth
{
    class Program
    {
        static void Main(string[] args)
        {
            string connString = "Host=147.93.68.244;Database=RuralAddressDb;Username=postgres;Password=Sushivivo182";
            using var conn = new NpgsqlConnection(connString);
            try
            {
                conn.Open();
                Console.WriteLine("Conectado ao banco de dados.");

                using var cmd = new NpgsqlCommand("SELECT \"Id\", \"Nome\", \"Cpf\", \"Nascimento\", \"TemBotaoPanico\" FROM \"Pessoas\" WHERE \"Nome\" ILIKE '%José Nogueira%' OR \"Cpf\" LIKE '%531.448.136-72%'", conn);
                
                using var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var id = reader["Id"];
                        var nome = reader["Nome"];
                        var cpf = reader["Cpf"];
                        var nascimento = reader["Nascimento"];
                        var temBotao = reader["TemBotaoPanico"];

                        Console.WriteLine("--------------------------------------------------");
                        Console.WriteLine($"ID: {id}");
                        Console.WriteLine($"Nome: {nome}");
                        Console.WriteLine($"CPF: {cpf}");
                        Console.WriteLine($"Nascimento (Raw): {nascimento}");
                        if (nascimento != DBNull.Value)
                        {
                            var dt = (DateTime)nascimento;
                            Console.WriteLine($"Nascimento (Formatado ddmmyyyy): {dt:ddMMyyyy}");
                            Console.WriteLine($"Nascimento (Kind): {dt.Kind}");
                        }
                        Console.WriteLine($"TemBotaoPanico: {temBotao}");
                        Console.WriteLine("--------------------------------------------------");
                    }
                }
                else
                {
                    Console.WriteLine("Nenhum usuário encontrado.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }
}
