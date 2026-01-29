using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class PanicChatMessage
{
    [Key]
    public int Id { get; set; }

    public int PanicAlertId { get; set; }
    [ForeignKey("PanicAlertId")]
    public PanicAlert? PanicAlert { get; set; }

    public DateTime DataHora { get; set; } = DateTime.UtcNow;

    [Required]
    public string Remetente { get; set; } = string.Empty; // "Usuario", "Sistema", "Admin"

    [Required]
    public string Mensagem { get; set; } = string.Empty;
}
