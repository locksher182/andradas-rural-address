using System.ComponentModel.DataAnnotations;

namespace RuralAddress.Core.Entities;

public class SystemParameter
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Group { get; set; } = string.Empty; // e.g., "VehicleType", "CropType"

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
