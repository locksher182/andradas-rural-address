using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RuralAddress.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCoordinatePrecision2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Propriedades",
                type: "numeric(18,8)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(11,8)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Propriedades",
                type: "numeric(18,8)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,8)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Longitude",
                table: "Propriedades",
                type: "numeric(11,8)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Latitude",
                table: "Propriedades",
                type: "numeric(10,8)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,8)",
                oldNullable: true);
        }
    }
}
