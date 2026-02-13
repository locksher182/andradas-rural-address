using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class PropriedadeValorAgregado
{
    [Key]
    public int Id { get; set; }

    public int PropriedadeId { get; set; }
    [ForeignKey("PropriedadeId")]
    public Propriedade Propriedade { get; set; } = null!;

    public int ValorAgregadoId { get; set; }
    [ForeignKey("ValorAgregadoId")]
    public RuralAddress.Core.Entities.SystemParameter ValorAgregado { get; set; } = null!;
}
