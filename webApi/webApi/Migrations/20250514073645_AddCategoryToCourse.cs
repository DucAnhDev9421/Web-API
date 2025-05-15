using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApi.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryToCourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "courses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_courses_CategoryId",
                table: "courses",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_courses_Categories_CategoryId",
                table: "courses",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_courses_Categories_CategoryId",
                table: "courses");

            migrationBuilder.DropIndex(
                name: "IX_courses_CategoryId",
                table: "courses");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "courses");
        }
    }
}
