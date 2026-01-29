using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RuralAddress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSystemParameters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Tipo",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "Cultivo",
                table: "Propriedades");

            migrationBuilder.AddColumn<int>(
                name: "TipoId",
                table: "Veiculos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CultivoId",
                table: "Propriedades",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SystemParameters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Group = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemParameters", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Veiculos_TipoId",
                table: "Veiculos",
                column: "TipoId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculos_SystemParameters_TipoId",
                table: "Veiculos",
                column: "TipoId",
                principalTable: "SystemParameters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Propriedades_SystemParameters_CultivoId",
                table: "Propriedades");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculos_SystemParameters_TipoId",
                table: "Veiculos");

            migrationBuilder.DropTable(
                name: "SystemParameters");

            migrationBuilder.DropIndex(
                name: "IX_Veiculos_TipoId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Propriedades_CultivoId",
                table: "Propriedades");

            migrationBuilder.DropColumn(
                name: "TipoId",
                table: "Veiculos");

            migrationBuilder.DropColumn(
                name: "CultivoId",
                table: "Propriedades");

            migrationBuilder.AddColumn<int>(
                name: "Tipo",
                table: "Veiculos",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Cultivo",
                table: "Propriedades",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
