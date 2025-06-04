using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace webApi.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId",
                table: "Coupons",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Coupons_CourseId",
                table: "Coupons",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Coupons_courses_CourseId",
                table: "Coupons",
                column: "CourseId",
                principalTable: "courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Coupons_courses_CourseId",
                table: "Coupons");

            migrationBuilder.DropIndex(
                name: "IX_Coupons_CourseId",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "CourseId",
                table: "Coupons");
        }
    }
}
