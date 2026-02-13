using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class Veiculo : IValidatableObject
{
    [Key]
    public int Id { get; set; }

    public int PropriedadeId { get; set; }
    [ForeignKey("PropriedadeId")]
    public Propriedade? Propriedade { get; set; }

    public int? TipoId { get; set; }
    [ForeignKey("TipoId")]
    public SystemParameter? Tipo { get; set; }

    [StringLength(50)]
    public string? TipoOutro { get; set; } // Usado se Tipo.Name == "Outro"

    [Required(ErrorMessage = "A Marca é obrigatória.")]
    [StringLength(50)]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "O Modelo é obrigatório.")]
    [StringLength(50)]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "A Cor é obrigatória.")]
    [StringLength(20)]
    public string Cor { get; set; } = string.Empty;

    [Required(ErrorMessage = "A placa é obrigatória.")]
    [StringLength(10)]
    [RuralAddress.Core.Validation.PlacaValidation]
    public string Placa { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [StringLength(100)]
    public string Descricao { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Note: This validation might need to be adjusted or moved to service layer 
        // if we can't guarantee Tipo is loaded or if "Outro" is dynamic.
        // For now, assuming "Outro" is a known system parameter name.
        if (Tipo?.Name == "Outro" && string.IsNullOrWhiteSpace(Descricao))
        {
            yield return new ValidationResult(
                "A descrição é obrigatória quando o tipo do veículo é 'Outro'.",
                new[] { nameof(Descricao) });
        }
    }
}
