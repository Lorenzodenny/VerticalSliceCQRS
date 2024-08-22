using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCartProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartProductId",
                table: "CartProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "CartProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CartProductId",
                table: "CartProducts");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "CartProducts");
        }
    }
}
