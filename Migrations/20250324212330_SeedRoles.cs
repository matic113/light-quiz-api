using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace light_quiz_api.Migrations
{
    /// <inheritdoc />
    public partial class SeedRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "id", "name", "normalized_name", "concurrency_stamp" },
                values: new object[] { Guid.NewGuid(), "student", "STUDENT", Guid.NewGuid().ToString() });

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "id", "name", "normalized_name", "concurrency_stamp" },
                values: new object[] { Guid.NewGuid(), "teacher", "TEACHER", Guid.NewGuid().ToString() });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"public.AspNetRoles\" WHERE \"name\" = 'student'");
            migrationBuilder.Sql("DELETE FROM \"public.AspNetRoles\" WHERE \"name\" = 'teacher'");
        }
    }
}
