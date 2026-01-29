using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuralAddress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkRotaToPropriedade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LinkRota",
                table: "Propriedades",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LinkRota",
                table: "Propriedades");
        }
    }
}
