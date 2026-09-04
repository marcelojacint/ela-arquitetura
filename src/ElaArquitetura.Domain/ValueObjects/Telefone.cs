namespace ElaArquitetura.Domain.ValueObjects;

public sealed record Telefone
{
    public string Numero { get; }

    private Telefone(string numero) => Numero = numero;

    public static bool TryCriar(string? valorBruto, out Telefone? telefone, out string? erro)
    {
        var digitos = new string((valorBruto ?? string.Empty).Where(char.IsDigit).ToArray());

        if (digitos.Length is < 10 or > 15)
        {
            telefone = null;
            erro = "Telefone deve conter entre 10 e 15 dígitos, incluindo DDD.";
            return false;
        }

        if (digitos.Length is 10 or 11)
            digitos = "55" + digitos;

        telefone = new Telefone("+" + digitos);
        erro = null;
        return true;
    }

    public string LinkWhatsApp() => $"https://wa.me/{Numero.TrimStart('+')}";

    public override string ToString() => Numero;
}
