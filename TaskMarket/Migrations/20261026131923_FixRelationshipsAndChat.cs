using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TaskMarket.Migrations
{
    /// <inheritdoc />
    public partial class FixRelationshipsAndChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceCategories_Contractors_ContractorId",
                table: "ServiceCategories");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_TaskMedias_mediaId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_Tasks_mediaId",
                table: "Tasks");

            migrationBuilder.DropIndex(
                name: "IX_ServiceCategories_ContractorId",
                table: "ServiceCategories");

            migrationBuilder.DropColumn(
                name: "mediaId",
                table: "Tasks");

            migrationBuilder.DropColumn(
                name: "ContractorId",
                table: "ServiceCategories");

            migrationBuilder.AlterColumn<int>(
                name: "ReceiverId",
                table: "Messages",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "mediaId",
                table: "Tasks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContractorId",
                table: "ServiceCategories",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ReceiverId",
                table: "Messages",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tasks_mediaId",
                table: "Tasks",
                column: "mediaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceCategories_ContractorId",
                table: "ServiceCategories",
                column: "ContractorId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceCategories_Contractors_ContractorId",
                table: "ServiceCategories",
                column: "ContractorId",
                principalTable: "Contractors",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_TaskMedias_mediaId",
                table: "Tasks",
                column: "mediaId",
                principalTable: "TaskMedias",
                principalColumn: "Id");
        }
    }
}
