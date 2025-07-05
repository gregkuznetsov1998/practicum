var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/health", (HttpContext httpContext) =>
{
    return "forecast";
})
.WithName("health");
app.Run();
