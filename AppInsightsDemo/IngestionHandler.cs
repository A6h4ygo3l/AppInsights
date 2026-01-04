using System.Text.Json;
using Microsoft.ApplicationInsights;

namespace AppInsightsDemo
{
    public class IngestionHandler
    {
        private readonly TelemetryClient _telemetryClient;
        private readonly ILogger<IngestionHandler> _logger;

        public IngestionHandler(TelemetryClient telemetryClient, ILogger<IngestionHandler> logger)
        {
            _telemetryClient = telemetryClient;
            _logger = logger;
        }
        public IResult Ingest(JsonElement jsonPayload)
        {
            // Log that we received a request
            _logger.LogInformation("Received JSON ingestion request.");

            // Create a dictionary for properties
            var properties = new Dictionary<string, string>();

            // Provide a string representation of the JSON
            string jsonString = jsonPayload.ToString();
            properties.Add("Payload", jsonString);

            // Track the event in Application Insights
            _telemetryClient.TrackEvent("JsonIngested", properties);

            return Results.Ok(new { Message = "JSON ingested successfully", Tracked = true });
        }
    }
}
