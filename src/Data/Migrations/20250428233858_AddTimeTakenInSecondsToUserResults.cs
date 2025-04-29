using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTimeTakenInSecondsToUserResults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "seconds_taken",
                table: "user_results",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "seconds_taken",
                table: "user_results");
        }
    }
}
