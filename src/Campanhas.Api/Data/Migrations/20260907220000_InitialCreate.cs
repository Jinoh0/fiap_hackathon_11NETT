using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campanhas.Api.Data.Migrations;

public partial class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "usuarios",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                NomeCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                SenhaHash = table.Column<string>(type: "text", nullable: false),
                Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_usuarios", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "campanhas",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                Descricao = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                MetaFinanceira = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                ValorArrecadado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                AtualizadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_campanhas", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "doacoes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                CampanhaId = table.Column<Guid>(type: "uuid", nullable: false),
                DoadorId = table.Column<Guid>(type: "uuid", nullable: false),
                ValorDoacao = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                CriadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                ProcessadoEm = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_doacoes", x => x.Id);
                table.ForeignKey(
                    name: "FK_doacoes_campanhas_CampanhaId",
                    column: x => x.CampanhaId,
                    principalTable: "campanhas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_doacoes_usuarios_DoadorId",
                    column: x => x.DoadorId,
                    principalTable: "usuarios",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_usuarios_Email",
            table: "usuarios",
            column: "Email",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_doacoes_CampanhaId",
            table: "doacoes",
            column: "CampanhaId");

        migrationBuilder.CreateIndex(
            name: "IX_doacoes_DoadorId",
            table: "doacoes",
            column: "DoadorId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "doacoes");
        migrationBuilder.DropTable(name: "campanhas");
        migrationBuilder.DropTable(name: "usuarios");
    }
}
