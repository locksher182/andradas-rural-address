using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuralAddress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPanicButtonToPessoa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "TemBotaoPanico",
                table: "Pessoas",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TemBotaoPanico",
                table: "Pessoas");
        }
    }
}
