using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarUrlToUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "avatar_url",
                table: "AspNetUsers",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "avatar_url",
                table: "AspNetUsers");
        }
    }
}
