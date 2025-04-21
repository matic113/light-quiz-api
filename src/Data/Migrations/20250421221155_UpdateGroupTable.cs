using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGroupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "invitation_id",
                table: "groups",
                newName: "created_by");

            migrationBuilder.AddColumn<string>(
                name: "short_code",
                table: "groups",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "short_code",
                table: "groups");

            migrationBuilder.RenameColumn(
                name: "created_by",
                table: "groups",
                newName: "invitation_id");
        }
    }
}
