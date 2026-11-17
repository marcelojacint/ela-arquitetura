using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <summary>
    /// Deixa os e-mails já gravados no mesmo formato que o domínio passou a usar
    /// (sem espaços nas pontas e em minúsculas), para o login não depender de maiúsculas.
    /// </summary>
    public partial class NormalizaEmailDosFuncionarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""UPDATE funcionarios SET "Email" = lower(btrim("Email")) WHERE "Email" <> lower(btrim("Email"));""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Não há como recuperar a grafia original; o e-mail normalizado continua válido.
        }
    }
}
