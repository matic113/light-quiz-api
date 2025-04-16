using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentAnswersTableToIncludeGradingResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "grading_confidence",
                table: "student_answers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "grading_feedback",
                table: "student_answers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "grading_rating",
                table: "student_answers",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "grading_confidence",
                table: "student_answers");

            migrationBuilder.DropColumn(
                name: "grading_feedback",
                table: "student_answers");

            migrationBuilder.DropColumn(
                name: "grading_rating",
                table: "student_answers");
        }
    }
}
