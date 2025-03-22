
using Microsoft.Extensions.Options;

namespace light_quiz_api
{
    public static class ConfigureApp
    {
        public static async Task Configure(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Light-Quiz Api");
            });

            app.UseHttpsRedirection();

            app.UseCors(app => app.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            await app.EnsureDatabaseCreated();
        }
        private static async Task EnsureDatabaseCreated(this WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.MigrateAsync();
        }
    }
}
