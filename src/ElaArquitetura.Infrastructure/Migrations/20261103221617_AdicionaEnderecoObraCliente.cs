using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaEnderecoObraCliente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoObra",
                table: "clientes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EnderecoObra",
                table: "clientes");
        }
    }
}
