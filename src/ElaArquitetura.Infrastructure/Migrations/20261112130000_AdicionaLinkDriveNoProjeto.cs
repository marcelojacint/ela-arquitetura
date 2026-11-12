using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaLinkDriveNoProjeto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkDrive",
                table: "projetos",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkDrive",
                table: "projetos");
        }
    }
}
