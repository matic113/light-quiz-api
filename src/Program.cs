using light_quiz_api;
using light_quiz_api.Services.Email;
using light_quiz_api.Services.Email.Models;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


builder.AddServices();

var app = builder.Build();

await app.Configure(builder.Configuration);

//Add support to logging request with SERILOG
app.UseSerilogRequestLogging();

app.MapGet("/health", () => "Hello World!")
    .WithTags("Health");

app.MapGet("/hello", () => "Hello World!")
    .WithTags("Health");

app.MapGet("/email/name", async (ResendService resend) =>
    {
        await resend.SendEmailWithTemplateAsync(
            "faressafi203@gmail.com",
            "Welcome To Light Quiz",
            "WelcomeEmail",
            new WelcomeEmailViewModel { AppName = "Light-Quiz", UserName = "Fares Mahmoud" });

        return Results.Ok("Email sent successfully!");
    }).WithTags("Email")
  .WithName("SendEmail");


app.Run();
