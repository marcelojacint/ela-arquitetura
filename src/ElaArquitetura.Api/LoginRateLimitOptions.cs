using System.ComponentModel.DataAnnotations;

namespace ElaArquitetura.Api;

/// <summary>
/// Quantas tentativas de login/refresh cada IP pode fazer por janela de tempo.
/// Padrão: 10 por minuto. Pode ser ajustado em "RateLimiting:Login" no appsettings.
/// </summary>
public class LoginRateLimitOptions
{
    public const string SectionName = "RateLimiting:Login";
    public const string PolicyName = "login";

    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; } = 10;

    [Range(1, 3600)]
    public int WindowSeconds { get; set; } = 60;
}
