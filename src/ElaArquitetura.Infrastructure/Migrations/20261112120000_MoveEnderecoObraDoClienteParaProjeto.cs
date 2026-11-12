using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <summary>
    /// O endereço da obra passa a ser do projeto: cada projeto do cliente pode ser em um lugar diferente.
    /// Os projetos já existentes recebem o endereço da obra que estava no cliente.
    /// </summary>
    public partial class MoveEnderecoObraDoClienteParaProjeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoObra",
                table: "projetos",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE projetos p
                SET "EnderecoObra" = c."EnderecoObra"
                FROM clientes c
                WHERE c."Id" = p."ClienteId" AND c."EnderecoObra" IS NOT NULL;
                """);

            migrationBuilder.DropColumn(
                name: "EnderecoObra",
                table: "clientes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoObra",
                table: "clientes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            // Volta para o cliente o endereço do projeto mais recente dele.
            migrationBuilder.Sql("""
                UPDATE clientes c
                SET "EnderecoObra" = (
                    SELECT p."EnderecoObra" FROM projetos p
                    WHERE p."ClienteId" = c."Id" AND p."EnderecoObra" IS NOT NULL
                    ORDER BY p."DataInicio" DESC
                    LIMIT 1);
                """);

            migrationBuilder.DropColumn(
                name: "EnderecoObra",
                table: "projetos");
        }
    }
}
