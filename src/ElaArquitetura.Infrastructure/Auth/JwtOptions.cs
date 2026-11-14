using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ElaArquitetura.Infrastructure.Auth;

public class JwtOptions : IValidatableObject
{
    public const string SectionName = "Jwt";

    /// <summary>HMAC-SHA256 exige chave de pelo menos 256 bits.</summary>
    public const int TamanhoMinimoChaveBytes = 32;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;

    [Range(1, 24 * 60)]
    public int ExpirationMinutes { get; set; } = 60;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Encoding.UTF8.GetByteCount(Key ?? string.Empty) < TamanhoMinimoChaveBytes)
            yield return new ValidationResult(
                $"Jwt:Key precisa ter pelo menos {TamanhoMinimoChaveBytes} bytes. Configure com 'dotnet user-secrets set Jwt:Key <chave>' " +
                "ou com a variável de ambiente Jwt__Key.",
                new[] { nameof(Key) });
    }
}
