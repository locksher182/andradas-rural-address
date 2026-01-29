using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Npgsql;

Console.WriteLine("Iniciando importação de dados de teste via CMD SQL...");

// Helper Method Inlined - ConvertToDecimalLocal
decimal? ConvertToDecimalLocal(string? dmsInput)
{
    if (string.IsNullOrWhiteSpace(dmsInput)) return null;

    var DmsRegex = new Regex(
        @"^([NSWE])\s*(\d{1,3})°\s*(\d{1,2})'\s*(\d+(\.\d+)?)(" + "\"" + ")?$", 
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    var match = DmsRegex.Match(dmsInput.Trim());
    if (!match.Success)
    {
        Console.WriteLine($"AVISO: Formato inválido: {dmsInput}");
        return null;
    }

    string direction = match.Groups[1].Value.ToUpper();
    int degrees = int.Parse(match.Groups[2].Value);
    int minutes = int.Parse(match.Groups[3].Value);
    decimal seconds = decimal.Parse(match.Groups[4].Value, CultureInfo.InvariantCulture);

    decimal decimalValue = degrees + (minutes / 60m) + (seconds / 3600m);

    if (direction == "S" || direction == "W")
    {
        decimalValue *= -1;
    }

    return Math.Round(decimalValue, 8);
}

string connString = "Host=147.93.68.244;Database=RuralAddressDb;Username=postgres;Password=Sushivivo182";

using var conn = new NpgsqlConnection(connString);
await conn.OpenAsync();
Console.WriteLine("Conexão SQL Aberta.");

// 2. Define Data
var records = new[]
{
    new {
        Codigo = "A01-0001", 
        Nome = "Lacir (\"Chilo\")",
        Coord1 = "S 22° 10' 52.3\"",
        Coord2 = "W 046° 31' 54.7\"",
        Link = "https://maps.app.goo.gl/XWf1tYZuM6vSX53C7",
        Bairro = "Setor I",
        Cpf = ""
    },
    new {
        Codigo = "A01-0002",
        Nome = "José Nogueira",
        Coord1 = "S 22° 10' 53.1\"",
        Coord2 = "W 046° 32' 02.1\"",
        Link = "https://maps.app.goo.gl/LU4nrr9gb8e89rpu9",
        Bairro = "Setor I",
        Cpf = "531.448.136-72"
    },
    new {
        Codigo = "A01-0003",
        Nome = "João Paulo de Lima",
        Coord1 = "S 22° 10' 20.0\"",
        Coord2 = "W 046° 32' 01.2\"",
        Link = "https://maps.app.goo.gl/nGAUjpVWLjJn5H699",
        Bairro = "Setor I",
        Cpf = ""
    }
};

foreach (var rec in records)
{
    try 
    {
        int propId = 0;

        // Check if exists
        using (var cmdCheck = new NpgsqlCommand("SELECT \"Id\" FROM \"Propriedades\" WHERE \"CepRural\" = @cep", conn))
        {
            cmdCheck.Parameters.AddWithValue("cep", rec.Codigo);
            var result = await cmdCheck.ExecuteScalarAsync();
            if (result != null)
            {
                propId = (int)result;
                Console.WriteLine($"  -> Propriedade {rec.Codigo} já existe (ID: {propId}). Usando existente.");
            }
        }

        decimal? lat = ConvertToDecimalLocal(rec.Coord1);
        decimal? lng = ConvertToDecimalLocal(rec.Coord2);

        int setor = 1; 
        if (rec.Bairro.Contains("II")) setor = 2;

        string nomeProp = $"Sítio do {rec.Nome.Split('(')[0].Replace("\"", "").Trim()}";
        if (nomeProp.Length > 255) nomeProp = nomeProp.Substring(0, 255);

        if (propId == 0) 
        {
            // Insert Prop
            string sqlProp = @"
                INSERT INTO ""Propriedades"" 
                (""CepRural"", ""NomePropriedade"", ""Setor"", ""Bairro"", ""Latitude"", ""Longitude"", ""LinkRota"", ""Observacoes"", ""Complemento"")
                VALUES 
                (@cep, @nome, @setor, @bairro, @lat, @lng, @link, @obs, @comp)
                RETURNING ""Id"";";

            using (var cmdProp = new NpgsqlCommand(sqlProp, conn))
            {
                cmdProp.Parameters.AddWithValue("cep", rec.Codigo);
                cmdProp.Parameters.AddWithValue("nome", nomeProp);
                cmdProp.Parameters.AddWithValue("setor", setor);
                cmdProp.Parameters.AddWithValue("bairro", rec.Bairro);
                cmdProp.Parameters.AddWithValue("lat", lat.HasValue ? (object)lat.Value : DBNull.Value);
                cmdProp.Parameters.AddWithValue("lng", lng.HasValue ? (object)lng.Value : DBNull.Value);
                cmdProp.Parameters.AddWithValue("link", rec.Link ?? (object)DBNull.Value);
                cmdProp.Parameters.AddWithValue("obs", "Importado via Script SQL");
                cmdProp.Parameters.AddWithValue("comp", "Importação Teste");

                propId = (int)(await cmdProp.ExecuteScalarAsync() ?? 0);
            }
            Console.WriteLine($"  -> Propriedade inserida. ID: {propId}");
        }

        if (propId > 0)
        {
             // Check person persistence? logic omitted, assume insert.
            string sqlPessoa = @"
                INSERT INTO ""Pessoas""
                (""Nome"", ""Cpf"", ""PropriedadeId"", ""TemBotaoPanico"", ""Rg"")
                VALUES
                (@nome, @cpf, @propId, @panico, @rg)";

             using (var cmdPessoa = new NpgsqlCommand(sqlPessoa, conn))
             {
                 cmdPessoa.Parameters.AddWithValue("nome", rec.Nome);
                 cmdPessoa.Parameters.AddWithValue("cpf", rec.Cpf); 
                 // cmdPessoa.Parameters.AddWithValue("tipo", "Proprietário"); // Removed
                 cmdPessoa.Parameters.AddWithValue("propId", propId);
                 cmdPessoa.Parameters.AddWithValue("panico", false);
                 cmdPessoa.Parameters.AddWithValue("rg", "");

                 await cmdPessoa.ExecuteNonQueryAsync();
             }
             Console.WriteLine($"  -> Pessoa inserida.");
        }

    }
    catch (Exception ex)
    {
        Console.WriteLine($"ERRO ao processar {rec.Nome}: {ex.Message}");
    }
}

Console.WriteLine("Importação via SQL Falhou? Não, Sucesso!");
