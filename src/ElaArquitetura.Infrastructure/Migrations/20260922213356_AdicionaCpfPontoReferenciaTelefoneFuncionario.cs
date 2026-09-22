using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCpfPontoReferenciaTelefoneFuncionario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "telefone",
                table: "funcionarios",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cpf",
                table: "clientes",
                type: "character varying(14)",
                maxLength: 14,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PontoReferencia",
                table: "clientes",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "telefone",
                table: "funcionarios");

            migrationBuilder.DropColumn(
                name: "Cpf",
                table: "clientes");

            migrationBuilder.DropColumn(
                name: "PontoReferencia",
                table: "clientes");
        }
    }
}
