using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddQuizShortCodeToResultsTableAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_results_user_id",
                table: "user_results");

            migrationBuilder.AddColumn<string>(
                name: "quiz_short_code",
                table: "user_results",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "ix_user_results_user_id_quiz_id",
                table: "user_results",
                columns: new[] { "user_id", "quiz_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_results_user_id_quiz_short_code",
                table: "user_results",
                columns: new[] { "user_id", "quiz_short_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_user_results_user_id_quiz_id",
                table: "user_results");

            migrationBuilder.DropIndex(
                name: "ix_user_results_user_id_quiz_short_code",
                table: "user_results");

            migrationBuilder.DropColumn(
                name: "quiz_short_code",
                table: "user_results");

            migrationBuilder.CreateIndex(
                name: "ix_user_results_user_id",
                table: "user_results",
                column: "user_id");
        }
    }
}
