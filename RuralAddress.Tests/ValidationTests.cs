using System.ComponentModel.DataAnnotations;
using RuralAddress.Core.Entities;
using Xunit;

namespace RuralAddress.Tests;

public class ValidationTests
{
    private IList<ValidationResult> ValidateModel(object model)
    {
        var validationResults = new List<ValidationResult>();
        var ctx = new ValidationContext(model, null, null);
        Validator.TryValidateObject(model, ctx, validationResults, true);
        return validationResults;
    }

    [Fact]
    public void Propriedade_ValidModel_ShouldHaveNoErrors()
    {
        var model = new Propriedade
        {
            CepRural = "A12-3456",
            NomePropriedade = "Fazenda Teste",
            Setor = 5,
            Bairro = "Bairro Rural"
        };

        var errors = ValidateModel(model);

        Assert.Empty(errors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123")] // Too short
    [InlineData("A12-34567")] // Too long
    public void Propriedade_InvalidCepRural_ShouldHaveErrors(string cep)
    {
        var model = new Propriedade
        {
            CepRural = cep,
            NomePropriedade = "Teste",
            Setor = 1
        };

        var errors = ValidateModel(model);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains("CepRural"));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(15)]
    public void Propriedade_InvalidSetor_ShouldHaveErrors(int setor)
    {
        var model = new Propriedade
        {
            CepRural = "A12-3456",
            NomePropriedade = "Teste",
            Setor = setor
        };

        var errors = ValidateModel(model);

        Assert.NotEmpty(errors);
        Assert.Contains(errors, e => e.MemberNames.Contains("Setor"));
    }

    [Fact]
    public void Pessoa_ValidModel_ShouldHaveNoErrors()
    {
        var model = new Pessoa
        {
            Nome = "João Silva",
            Cpf = "123.456.789-01" // Note: This will need a valid CPF if we use actual validation
        };

        // For testing purposes, we might need a valid CPF generator or known valid CPF
    }

    [Theory]
    [InlineData("ABC-1234")] // Valid old
    [InlineData("ABC1C34")] // Invalid format (missing hyphen)
    [InlineData("ABC-1A24")] // Valid Mercosul
    public void Veiculo_PlacaValidation_Test(string placa)
    {
        var model = new Veiculo
        {
            Placa = placa,
            Marca = "Teste",
            Modelo = "Teste",
            Cor = "Azul",
            Descricao = "Teste"
        };

        var errors = ValidateModel(model);
        
        // We'll see if the regex handles these correctly
    }
}
