using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace RuralAddress.Core.Validation;

public class PlacaValidationAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value == null || string.IsNullOrEmpty(value.ToString()))
        {
            return ValidationResult.Success; // Allow empty, use [Required] if needed
        }

        var placa = value.ToString()!.ToUpper().Trim();

        // Format 1: LLL-NNNN (Old)
        // Format 2: LLL-NLNN (Mercosul)
        // Regex explanation:
        // ^[A-Z]{3}  : Starts with 3 letters
        // -          : Hyphen
        // (          : Start group
        //   \d{4}    : 4 digits (Old)
        //   |        : OR
        //   \d[A-Z]\d{2} : 1 digit, 1 letter, 2 digits (Mercosul)
        // )          : End group
        // $          : End of string

        var regex = new Regex(@"^[A-Z]{3}-(\d{4}|\d[A-Z]\d{2})$");

        if (!regex.IsMatch(placa))
        {
            return new ValidationResult("A placa deve estar no formato LLL-NNNN ou LLL-NLNN (Onde L= Letra e N=Número).");
        }

        return ValidationResult.Success;
    }
}
