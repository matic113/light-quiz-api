using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateStudentResultsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "correct_questions",
                table: "user_results",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "total_question",
                table: "user_results",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "correct_questions",
                table: "user_results");

            migrationBuilder.DropColumn(
                name: "total_question",
                table: "user_results");
        }
    }
}
