using AppInsightsDemo;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces));

// Add services to the container.
builder.Services.AddSingleton<TestHandler>();
builder.Services.AddSingleton<IngestionHandler>();
builder.Services.AddSingleton<SerilogHandler>();
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

app.MapGet("/api/fetch", (IngestionHandler handler) =>
{
    return handler.SearchLogs();
});

// Serilog Endpoints
app.MapPost("/api/serilog/ingest", (SerilogHandler handler, System.Text.Json.JsonElement jsonPayload) =>
{
    return handler.Ingest(jsonPayload);
});

app.MapGet("/api/serilog/fetch", (SerilogHandler handler, string? tenantId, string? correlationId) =>
{
    return handler.Fetch();
});


app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

