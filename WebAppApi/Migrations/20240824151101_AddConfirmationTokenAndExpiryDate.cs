using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebAppApi.Migrations
{
    /// <inheritdoc />
    public partial class AddConfirmationTokenAndExpiryDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConfirmationToken",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiryDate",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConfirmationToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "TokenExpiryDate",
                table: "AspNetUsers");
        }
    }
}
