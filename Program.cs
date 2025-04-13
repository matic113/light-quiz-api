using light_quiz_api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();

var app = builder.Build();

await app.Configure();

app.MapGet("/health", () => "Hello World!")
    .WithTags("Health");

app.MapGet("/hello", () => "Hello World!")
    .WithTags("Health");

app.Run();
