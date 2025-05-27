using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveShadowColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_group_members_users_app_user_id",
                table: "group_members");

            migrationBuilder.DropForeignKey(
                name: "fk_quiz_progresses_users_app_user_id",
                table: "quiz_progresses");

            migrationBuilder.DropForeignKey(
                name: "fk_student_answers_users_app_user_id",
                table: "student_answers");

            migrationBuilder.DropForeignKey(
                name: "fk_user_results_users_app_user_id",
                table: "user_results");

            migrationBuilder.DropIndex(
                name: "ix_user_results_app_user_id",
                table: "user_results");

            migrationBuilder.DropIndex(
                name: "ix_student_answers_app_user_id",
                table: "student_answers");

            migrationBuilder.DropIndex(
                name: "ix_quiz_progresses_app_user_id",
                table: "quiz_progresses");

            migrationBuilder.DropIndex(
                name: "ix_group_members_app_user_id",
                table: "group_members");

            migrationBuilder.DropColumn(
                name: "app_user_id",
                table: "user_results");

            migrationBuilder.DropColumn(
                name: "app_user_id",
                table: "student_answers");

            migrationBuilder.DropColumn(
                name: "app_user_id",
                table: "quiz_progresses");

            migrationBuilder.DropColumn(
                name: "app_user_id",
                table: "group_members");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "app_user_id",
                table: "user_results",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "app_user_id",
                table: "student_answers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "app_user_id",
                table: "quiz_progresses",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "app_user_id",
                table: "group_members",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "ix_user_results_app_user_id",
                table: "user_results",
                column: "app_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_student_answers_app_user_id",
                table: "student_answers",
                column: "app_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_progresses_app_user_id",
                table: "quiz_progresses",
                column: "app_user_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_members_app_user_id",
                table: "group_members",
                column: "app_user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_group_members_users_app_user_id",
                table: "group_members",
                column: "app_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_quiz_progresses_users_app_user_id",
                table: "quiz_progresses",
                column: "app_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_student_answers_users_app_user_id",
                table: "student_answers",
                column: "app_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "fk_user_results_users_app_user_id",
                table: "user_results",
                column: "app_user_id",
                principalTable: "AspNetUsers",
                principalColumn: "id");
        }
    }
}
