using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class PropriedadeTipoTrabalho
{
    [Key]
    public int Id { get; set; }

    public int PropriedadeId { get; set; }
    [ForeignKey("PropriedadeId")]
    public Propriedade Propriedade { get; set; } = null!;

    public int TipoTrabalhoId { get; set; }
    [ForeignKey("TipoTrabalhoId")]
    public RuralAddress.Core.Entities.SystemParameter TipoTrabalho { get; set; } = null!;
}
