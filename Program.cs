using light_quiz_api;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

//Add support to logging with SERILOG
builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));


builder.AddServices();

var app = builder.Build();

await app.Configure();

//Add support to logging request with SERILOG
app.UseSerilogRequestLogging();


app.MapGet("/health", () => "Hello World!")
    .WithTags("Health");

app.MapGet("/hello", () => "Hello World!")
    .WithTags("Health");

app.Run();
