using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddShortCodeToQuizTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "short_code",
                table: "quizzes",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quizzes_ShortCode",
                table: "quizzes",
                column: "short_code",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Quizzes_ShortCode",
                table: "quizzes");

            migrationBuilder.DropColumn(
                name: "short_code",
                table: "quizzes");
        }
    }
}
