using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class PropriedadeCultivo
{
    [Key]
    public int Id { get; set; }

    public int PropriedadeId { get; set; }
    [ForeignKey("PropriedadeId")]
    public Propriedade Propriedade { get; set; } = null!;

    public int CultivoId { get; set; }
    [ForeignKey("CultivoId")]
    public SystemParameter Cultivo { get; set; } = null!;
}
