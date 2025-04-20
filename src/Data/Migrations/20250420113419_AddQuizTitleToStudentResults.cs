using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizTitleToStudentResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "quiz_title",
                table: "user_results",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "quiz_title",
                table: "user_results");
        }
    }
}
