using light_quiz_api;

var builder = WebApplication.CreateBuilder(args);

builder.AddServices();

var app = builder.Build();

await app.Configure();

app.MapGet("/", () => "Hello World!")
    .WithTags("TESTING");

app.Run();
