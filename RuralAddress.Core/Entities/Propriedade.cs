using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RuralAddress.Core.Entities;

public class Propriedade
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "O CEP Rural é obrigatório.")]
    [StringLength(8, ErrorMessage = "O CEP Rural deve ter 8 caracteres.")]
    public string CepRural { get; set; } = string.Empty; // A00-0000

    [Required(ErrorMessage = "O Nome da Propriedade é obrigatório.")]
    [StringLength(255, ErrorMessage = "O Nome da Propriedade deve ter no máximo 255 caracteres.")]
    public string NomePropriedade { get; set; } = string.Empty;

    [Range(1, 14, ErrorMessage = "O Setor deve ser entre 1 e 14.")]
    public int Setor { get; set; }

    [StringLength(100)]
    public string Bairro { get; set; } = string.Empty;

    [StringLength(255)]
    public string Complemento { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Latitude { get; set; }

    [Column(TypeName = "decimal(18,8)")]
    public decimal? Longitude { get; set; }


    public string Observacoes { get; set; } = string.Empty;

    [StringLength(500)]
    public string? FotoCaminho { get; set; }

    [StringLength(500)]
    public string? LinkRota { get; set; }

    [NotMapped]
    public string EnderecoCompleto => $"{NomePropriedade}, {Bairro}, Setor {Setor}, CEP {CepRural}";

    [NotMapped]
    public string LinkGoogleMaps => (Latitude.HasValue && Longitude.HasValue) 
        ? $"https://www.google.com/maps/search/?api=1&query={Latitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)},{Longitude.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}"
        : "#";

    // Relacionamentos
    public List<PropriedadeCultivo> Cultivos { get; set; } = new();
    public List<Pessoa> Pessoas { get; set; } = new();
    public List<Veiculo> Veiculos { get; set; } = new();
}
