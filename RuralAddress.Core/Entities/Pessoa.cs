using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class Pessoa
{
    [Key]
    public int Id { get; set; }

    public int PropriedadeId { get; set; }
    [ForeignKey("PropriedadeId")]
    public Propriedade? Propriedade { get; set; }

    [Required(ErrorMessage = "O Nome é obrigatório.")]
    [StringLength(255, ErrorMessage = "O Nome deve ter no máximo 255 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    public DateTime? Nascimento { get; set; }

    [StringLength(14, ErrorMessage = "O CPF deve ter no máximo 14 caracteres.")]
    [RuralAddress.Core.Validation.CpfValidation(ErrorMessage = "CPF inválido.")]
    public string Cpf { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "O RG deve ter no máximo 20 caracteres.")]
    public string Rg { get; set; } = string.Empty;

    public bool TemBotaoPanico { get; set; } = false;
}
