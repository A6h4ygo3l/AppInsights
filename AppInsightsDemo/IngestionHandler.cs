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

        public async Task<IResult> SearchLogs()
        {
            try
            {
                // Base KQL Query
                // We are looking for custom events named 'JsonIngested'
                string kql = @"
                    AppEvents 
                    | where Name == 'JsonIngested'";
        
                // Execute Query
                Response<LogsQueryResult> response = await _logsQueryClient.QueryWorkspaceAsync(
                    _workspaceId,
                    kql,
                    new QueryTimeRange(TimeSpan.FromDays(7))); // Query last 7 days by default
        
                var results = new List<string>();
                foreach (var row in response.Value.Table.Rows)
                {
                    // row[0] is Payload based on projection
                    string jsonString = row[3].ToString();
                    results.Add(jsonString);
                }
        
                return Results.Ok(results);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Error querying App Insights: {ex.Message}");
            }
        }
    }
}

