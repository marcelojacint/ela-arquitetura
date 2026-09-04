namespace ElaArquitetura.Domain.Entities;

public class Entrega
{
    public Guid Id { get; private set; }
    public Guid ProjetoId { get; private set; }
    public string LinkDrive { get; private set; } = string.Empty;
    public DateTime DataEnvio { get; private set; }
    public bool EnviadoParaWhatsapp { get; private set; }
    public DateTime? DataEnvioWhatsapp { get; private set; }

    protected Entrega()
    {
    }

    public Entrega(Guid projetoId, string linkDrive)
    {
        Id = Guid.NewGuid();
        ProjetoId = projetoId;
        LinkDrive = linkDrive;
        DataEnvio = DateTime.UtcNow;
        EnviadoParaWhatsapp = false;
    }

    public void RegistrarEnvioWhatsapp()
    {
        EnviadoParaWhatsapp = true;
        DataEnvioWhatsapp = DateTime.UtcNow;
    }
}
