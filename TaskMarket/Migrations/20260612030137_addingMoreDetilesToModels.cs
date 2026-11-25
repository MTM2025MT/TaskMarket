using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMarket.Migrations
{
    /// <inheritdoc />
    public partial class addingMoreDetilesToModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "Tasks",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "Tasks",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "mediaId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Latitude",
                table: "JobOffers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Location_Longitude",
                table: "JobOffers",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxPrice",
                table: "JobOffers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPrice",
                table: "JobOffers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_mediaId",
                table: "Tasks",
                column: "mediaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskMedias_mediaId",
                table: "Tasks",
                column: "mediaId",
                principalTable: "TaskMedias",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskMedias_mediaId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_mediaId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "mediaId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "Location_Latitude",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "Location_Longitude",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "MaxPrice",
                table: "JobOffers");

            migrationBuilder.DropColumn(
                name: "MinPrice",
                table: "JobOffers");
        }
    }
}
