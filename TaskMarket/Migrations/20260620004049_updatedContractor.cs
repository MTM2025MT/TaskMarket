using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMarket.Migrations
{
    /// <inheritdoc />
    public partial class updatedContractor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AboutMe",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "CompletedTaskCount",
                table: "Contractors",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProfilePicturePath",
                table: "Contractors",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AboutMe",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "CompletedTaskCount",
                table: "Contractors");

            migrationBuilder.DropColumn(
                name: "ProfilePicturePath",
                table: "Contractors");
        }
    }
}
