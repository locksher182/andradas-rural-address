using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RuralAddress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropriedadeCultivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Propriedades_SystemParameters_CultivoId",
                table: "Propriedades");

            migrationBuilder.DropIndex(
                name: "IX_Propriedades_CultivoId",
                table: "Propriedades");

            migrationBuilder.DropColumn(
                name: "CultivoId",
                table: "Propriedades");

            migrationBuilder.CreateTable(
                name: "PropriedadeCultivos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PropriedadeId = table.Column<int>(type: "integer", nullable: false),
                    CultivoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropriedadeCultivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropriedadeCultivos_Propriedades_PropriedadeId",
                        column: x => x.PropriedadeId,
                        principalTable: "Propriedades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PropriedadeCultivos_SystemParameters_CultivoId",
                        column: x => x.CultivoId,
                        principalTable: "SystemParameters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultivos_CultivoId",
                table: "PropriedadeCultivos",
                column: "CultivoId");

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadeCultivos_PropriedadeId",
                table: "PropriedadeCultivos",
                column: "PropriedadeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropriedadeCultivos");

            migrationBuilder.AddColumn<int>(
                name: "CultivoId",
                table: "Propriedades",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Propriedades_CultivoId",
                table: "Propriedades",
                column: "CultivoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Propriedades_SystemParameters_CultivoId",
                table: "Propriedades",
                column: "CultivoId",
                principalTable: "SystemParameters",
                principalColumn: "Id");
        }
    }
}
