using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FuncionarioCadastroSemAcessoComDataNascimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_funcionarios_Email",
                table: "funcionarios");

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "funcionarios",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cargo",
                table: "funcionarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataNascimento",
                table: "funcionarios",
                type: "date",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_funcionarios_Email",
                table: "funcionarios",
                column: "Email",
                unique: true,
                filter: "\"Ativo\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_funcionarios_Email",
                table: "funcionarios");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "funcionarios");

            migrationBuilder.AlterColumn<string>(
                name: "SenhaHash",
                table: "funcionarios",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cargo",
                table: "funcionarios",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_funcionarios_Email",
                table: "funcionarios",
                column: "Email",
                unique: true);
        }
    }
}
