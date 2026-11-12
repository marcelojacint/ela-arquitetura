using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <summary>
    /// Regra de negócio: todo cliente tem um projeto, e o projeto nasce com o checklist de todas as etapas.
    /// Antes o projeto era criado à parte e o checklist só era gerado ao entrar em cada etapa; esta migration
    /// acerta os dados já existentes (só dados, o modelo não muda).
    /// </summary>
    public partial class ProvisionaProjetoEChecklistCompletoParaClientesExistentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clientes sem projeto ganham um, começando em "Estudos Preliminares"
            // (a mesma etapa em que a aplicação abre um projeto novo).
            migrationBuilder.Sql("""
                INSERT INTO projetos ("Id", "ClienteId", "Titulo", "Status", "DataInicio", "DataConclusao", "EtapaAtualId")
                SELECT gen_random_uuid(), c."Id", c."Nome", 'EmAndamento', now(), NULL, '00000000-0000-0000-0000-000000000002'
                FROM clientes c
                WHERE NOT EXISTS (SELECT 1 FROM projetos p WHERE p."ClienteId" = c."Id");
                """);

            // Todo projeto recebe os itens das sub-etapas que ainda não tem, em todas as etapas.
            migrationBuilder.Sql("""
                INSERT INTO checklist_itens ("Id", "ProjetoId", "EtapaId", "SubEtapaId", "Descricao", "Concluido", "ConcluidoPor", "DataConclusao")
                SELECT gen_random_uuid(), p."Id", s."EtapaId", s."Id", s."Nome", false, NULL, NULL
                FROM projetos p
                CROSS JOIN sub_etapas s
                WHERE NOT EXISTS (
                    SELECT 1 FROM checklist_itens i
                    WHERE i."ProjetoId" = p."Id" AND i."SubEtapaId" = s."Id");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Migration só de dados: não há como distinguir o que foi gerado aqui do que veio do uso normal.
        }
    }
}
