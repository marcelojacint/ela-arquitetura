using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElaArquitetura.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InicialComSeedDeEtapas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "clientes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    telefone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Endereco = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "etapas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Opcional = table.Column<bool>(type: "boolean", nullable: false),
                    Final = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_etapas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "funcionarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Cargo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SenhaHash = table.Column<string>(type: "text", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_funcionarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "projetos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClienteId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EtapaAtualId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projetos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_projetos_clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_projetos_etapas_EtapaAtualId",
                        column: x => x.EtapaAtualId,
                        principalTable: "etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "sub_etapas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EtapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nome = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sub_etapas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_sub_etapas_etapas_EtapaId",
                        column: x => x.EtapaId,
                        principalTable: "etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "entregas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjetoId = table.Column<Guid>(type: "uuid", nullable: false),
                    LinkDrive = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EnviadoParaWhatsapp = table.Column<bool>(type: "boolean", nullable: false),
                    DataEnvioWhatsapp = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entregas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entregas_projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "projeto_funcionarios",
                columns: table => new
                {
                    ProjetoId = table.Column<Guid>(type: "uuid", nullable: false),
                    FuncionarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    PapelNoProjeto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_projeto_funcionarios", x => new { x.ProjetoId, x.FuncionarioId });
                    table.ForeignKey(
                        name: "FK_projeto_funcionarios_funcionarios_FuncionarioId",
                        column: x => x.FuncionarioId,
                        principalTable: "funcionarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_projeto_funcionarios_projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "checklist_itens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProjetoId = table.Column<Guid>(type: "uuid", nullable: false),
                    EtapaId = table.Column<Guid>(type: "uuid", nullable: false),
                    SubEtapaId = table.Column<Guid>(type: "uuid", nullable: true),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Concluido = table.Column<bool>(type: "boolean", nullable: false),
                    ConcluidoPor = table.Column<Guid>(type: "uuid", nullable: true),
                    DataConclusao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_checklist_itens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_checklist_itens_etapas_EtapaId",
                        column: x => x.EtapaId,
                        principalTable: "etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_checklist_itens_projetos_ProjetoId",
                        column: x => x.ProjetoId,
                        principalTable: "projetos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_checklist_itens_sub_etapas_SubEtapaId",
                        column: x => x.SubEtapaId,
                        principalTable: "sub_etapas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "etapas",
                columns: new[] { "Id", "Final", "Nome", "Opcional", "Ordem" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), false, "Cadastro do Cliente", false, 1 },
                    { new Guid("00000000-0000-0000-0000-000000000002"), false, "Estudos Preliminares", false, 2 },
                    { new Guid("00000000-0000-0000-0000-000000000003"), false, "Anteprojeto", false, 3 },
                    { new Guid("00000000-0000-0000-0000-000000000004"), false, "Projeto Executivo", false, 4 },
                    { new Guid("00000000-0000-0000-0000-000000000005"), false, "Relatório de Obra", true, 5 },
                    { new Guid("00000000-0000-0000-0000-000000000006"), true, "Conclusão e Entrega", false, 6 }
                });

            migrationBuilder.InsertData(
                table: "sub_etapas",
                columns: new[] { "Id", "EtapaId", "Nome", "Ordem" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000101"), new Guid("00000000-0000-0000-0000-000000000002"), "Briefing", 1 },
                    { new Guid("00000000-0000-0000-0000-000000000102"), new Guid("00000000-0000-0000-0000-000000000002"), "Levantamento em Locação", 2 },
                    { new Guid("00000000-0000-0000-0000-000000000103"), new Guid("00000000-0000-0000-0000-000000000002"), "Estudo de Layout", 3 },
                    { new Guid("00000000-0000-0000-0000-000000000201"), new Guid("00000000-0000-0000-0000-000000000004"), "Executivo de Obra", 1 },
                    { new Guid("00000000-0000-0000-0000-000000000202"), new Guid("00000000-0000-0000-0000-000000000004"), "Detalhamento de Marcenaria", 2 },
                    { new Guid("00000000-0000-0000-0000-000000000203"), new Guid("00000000-0000-0000-0000-000000000004"), "Detalhamento de Marmoraria", 3 },
                    { new Guid("00000000-0000-0000-0000-000000000204"), new Guid("00000000-0000-0000-0000-000000000004"), "Memoriais Descritivos", 4 },
                    { new Guid("00000000-0000-0000-0000-000000000205"), new Guid("00000000-0000-0000-0000-000000000004"), "Imagens", 5 },
                    { new Guid("00000000-0000-0000-0000-000000000206"), new Guid("00000000-0000-0000-0000-000000000004"), "Maquete 3D", 6 },
                    { new Guid("00000000-0000-0000-0000-000000000207"), new Guid("00000000-0000-0000-0000-000000000004"), "Render", 7 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_checklist_itens_EtapaId",
                table: "checklist_itens",
                column: "EtapaId");

            migrationBuilder.CreateIndex(
                name: "IX_checklist_itens_ProjetoId_EtapaId",
                table: "checklist_itens",
                columns: new[] { "ProjetoId", "EtapaId" });

            migrationBuilder.CreateIndex(
                name: "IX_checklist_itens_SubEtapaId",
                table: "checklist_itens",
                column: "SubEtapaId");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_Nome",
                table: "clientes",
                column: "Nome");

            migrationBuilder.CreateIndex(
                name: "IX_clientes_telefone",
                table: "clientes",
                column: "telefone");

            migrationBuilder.CreateIndex(
                name: "IX_entregas_ProjetoId",
                table: "entregas",
                column: "ProjetoId");

            migrationBuilder.CreateIndex(
                name: "IX_funcionarios_Email",
                table: "funcionarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_projeto_funcionarios_FuncionarioId",
                table: "projeto_funcionarios",
                column: "FuncionarioId");

            migrationBuilder.CreateIndex(
                name: "IX_projetos_ClienteId",
                table: "projetos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_projetos_EtapaAtualId",
                table: "projetos",
                column: "EtapaAtualId");

            migrationBuilder.CreateIndex(
                name: "IX_projetos_Status",
                table: "projetos",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_sub_etapas_EtapaId",
                table: "sub_etapas",
                column: "EtapaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "checklist_itens");

            migrationBuilder.DropTable(
                name: "entregas");

            migrationBuilder.DropTable(
                name: "projeto_funcionarios");

            migrationBuilder.DropTable(
                name: "sub_etapas");

            migrationBuilder.DropTable(
                name: "funcionarios");

            migrationBuilder.DropTable(
                name: "projetos");

            migrationBuilder.DropTable(
                name: "clientes");

            migrationBuilder.DropTable(
                name: "etapas");
        }
    }
}
