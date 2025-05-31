using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApi.Migrations
{
    /// <inheritdoc />
    public partial class FixCourseIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "coursesId",
                table: "Ratings",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_coursesId",
                table: "Ratings",
                column: "coursesId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ratings_courses_coursesId",
                table: "Ratings",
                column: "coursesId",
                principalTable: "courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ratings_courses_coursesId",
                table: "Ratings");

            migrationBuilder.DropIndex(
                name: "IX_Ratings_coursesId",
                table: "Ratings");

            migrationBuilder.DropColumn(
                name: "coursesId",
                table: "Ratings");
        }
    }
}
