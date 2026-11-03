using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlinhaEtapasESubEtapasAoFigma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // "Render" sai do fluxo; itens já criados a partir dela viram itens avulsos em vez de bloquear a exclusão.
            migrationBuilder.Sql(
                "UPDATE checklist_itens SET \"SubEtapaId\" = NULL WHERE \"SubEtapaId\" = '00000000-0000-0000-0000-000000000207';");

            migrationBuilder.DeleteData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000207"));

            migrationBuilder.UpdateData(
                table: "etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Opcional",
                value: true);

            migrationBuilder.UpdateData(
                table: "etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                column: "Nome",
                value: "Acompanhamentos");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000201"),
                column: "Nome",
                value: "Projeto Executivo");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000203"),
                column: "Nome",
                value: "Detalhamento de Pedras e Bancadas");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000204"),
                column: "Nome",
                value: "Memorial Descritivo");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000205"),
                column: "Nome",
                value: "Apresentação 3D");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000206"),
                columns: new[] { "EtapaId", "Ordem" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000003"), 1 });

            migrationBuilder.InsertData(
                table: "sub_etapas",
                columns: new[] { "Id", "EtapaId", "Nome", "Ordem" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000501"), new Guid("00000000-0000-0000-0000-000000000005"), "Relatório de Obra", 1 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000501"));

            migrationBuilder.UpdateData(
                table: "etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "Opcional",
                value: false);

            migrationBuilder.UpdateData(
                table: "etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000005"),
                column: "Nome",
                value: "Relatório de Obra");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000201"),
                column: "Nome",
                value: "Executivo de Obra");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000203"),
                column: "Nome",
                value: "Detalhamento de Marmoraria");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000204"),
                column: "Nome",
                value: "Memoriais Descritivos");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000205"),
                column: "Nome",
                value: "Imagens");

            migrationBuilder.UpdateData(
                table: "sub_etapas",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000206"),
                columns: new[] { "EtapaId", "Ordem" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000004"), 6 });

            migrationBuilder.InsertData(
                table: "sub_etapas",
                columns: new[] { "Id", "EtapaId", "Nome", "Ordem" },
                values: new object[] { new Guid("00000000-0000-0000-0000-000000000207"), new Guid("00000000-0000-0000-0000-000000000004"), "Render", 7 });
        }
    }
}
