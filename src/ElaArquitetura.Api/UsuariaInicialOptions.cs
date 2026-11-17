using System.ComponentModel.DataAnnotations;

namespace ElaArquitetura.Api;

/// <summary>
/// Dados de acesso da usuária do sistema (a dona do escritório), seção "UsuariaInicial".
/// O e-mail fica no appsettings; a senha nunca vai para o repositório: em desenvolvimento fica no
/// User Secrets e em produção na variável de ambiente UsuariaInicial__Senha.
/// </summary>
public class UsuariaInicialOptions : IValidatableObject
{
    public const string SectionName = "UsuariaInicial";
    public const int TamanhoMinimoSenha = 8;

    public string Nome { get; set; } = "Administradora";
    public string Cargo { get; set; } = "Proprietaria";
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;

    /// <summary>Sem e-mail e senha o seed não faz nada (a API sobe, mas avisa que ninguém consegue entrar).</summary>
    public bool Configurada => !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrEmpty(Senha);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(Senha))
            yield break;

        if (string.IsNullOrWhiteSpace(Email))
            yield return new ValidationResult("UsuariaInicial:Email é obrigatório quando a senha é informada.", new[] { nameof(Email) });

        if (Senha.Length < TamanhoMinimoSenha)
            yield return new ValidationResult(
                $"UsuariaInicial:Senha precisa ter pelo menos {TamanhoMinimoSenha} caracteres.", new[] { nameof(Senha) });

        if (string.IsNullOrWhiteSpace(Nome))
            yield return new ValidationResult("UsuariaInicial:Nome não pode ficar vazio.", new[] { nameof(Nome) });
    }
}
