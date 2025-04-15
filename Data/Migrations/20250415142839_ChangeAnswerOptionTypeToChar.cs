using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAnswerOptionTypeToChar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "answer_option",
                table: "student_answers");

            migrationBuilder.AlterColumn<string>(
                name: "answer_text",
                table: "student_answers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AddColumn<char>(
                name: "answer_option_letter",
                table: "student_answers",
                type: "character(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "answer_option_letter",
                table: "student_answers");

            migrationBuilder.AlterColumn<string>(
                name: "answer_text",
                table: "student_answers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "answer_option",
                table: "student_answers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}
