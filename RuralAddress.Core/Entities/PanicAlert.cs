using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class PanicAlert
{
    [Key]
    public int Id { get; set; }

    public int PessoaId { get; set; }
    [ForeignKey("PessoaId")]
    public Pessoa? Pessoa { get; set; }

    public DateTime DataHora { get; set; } = DateTime.UtcNow;

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public bool Resolvido { get; set; } = false;

    public List<PanicChatMessage> ChatMessages { get; set; } = new();
}
