using AppInsightsDemo;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<TestHandler>();
builder.Services.AddSingleton<IngestionHandler>();
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

//Simplest redirection
app.MapGet("/", () => "Hello World!");

//Default example from VS
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

//Redirection to service 
app.MapGet("/diversion", (TestHandler testHandler) =>
{
    return testHandler.GetHello();
});

// Map the Post request directly
app.MapPost("/api/ingest", (IngestionHandler handler, System.Text.Json.JsonElement jsonPayload) =>
{
    return handler.Ingest(jsonPayload);
});

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
