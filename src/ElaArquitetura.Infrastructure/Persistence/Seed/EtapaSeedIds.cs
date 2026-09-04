namespace ElaArquitetura.Infrastructure.Persistence.Seed;

/// <summary>
/// Ids fixos das 6 etapas do fluxo (PRD seção 6) — precisam ser constantes porque
/// alimentam o HasData das migrations, não o cadastro do usuário.
/// </summary>
internal static class EtapaSeedIds
{
    public static readonly Guid CadastroDoCliente = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid EstudosPreliminares = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid Anteprojeto = Guid.Parse("00000000-0000-0000-0000-000000000003");
    public static readonly Guid ProjetoExecutivo = Guid.Parse("00000000-0000-0000-0000-000000000004");
    public static readonly Guid RelatorioDeObra = Guid.Parse("00000000-0000-0000-0000-000000000005");
    public static readonly Guid ConclusaoEEntrega = Guid.Parse("00000000-0000-0000-0000-000000000006");
}
