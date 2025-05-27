
using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;

namespace light_quiz_api
{
    public static class ConfigureApp
    {
        public static async Task Configure(this WebApplication app, IConfiguration configuration)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Light-Quiz Api");
            });

            app.UseHttpsRedirection();

            app.UseCors(app => app.AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader()
                                .WithExposedHeaders("Location"));

            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();

            string hangfirePassword = configuration["Hangfire:DashboardPassword"] ?? "admin";

            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
                {
                    RequireSsl = true,
                    SslRedirect = false,
                    LoginCaseSensitive = true,
                    Users = new []
                    {
                        new BasicAuthAuthorizationUser
                        {
                            Login = "admin",
                            PasswordClear =  hangfirePassword,
                        }
                    }

                })}
            });


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
