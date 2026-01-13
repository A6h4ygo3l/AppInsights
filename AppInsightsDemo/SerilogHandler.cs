using System.Text.Json;
using Azure.Monitor.Query.Models;
using Azure.Monitor.Query;
using Azure;
using Azure.Identity;
using Serilog;

namespace AppInsightsDemo
{
    public class SerilogHandler
    {
        private readonly string _workspaceId;
        private readonly LogsQueryClient _logsQueryClient;
        private readonly IConfiguration _configuration;

        public SerilogHandler(IConfiguration configuration)
        {
            _configuration = configuration;
            _workspaceId = configuration["ApplicationInsights:WorkspaceId"]
                ?? throw new InvalidOperationException("Config 'ApplicationInsights:WorkspaceId' not found.");

            _logsQueryClient = new LogsQueryClient(new DefaultAzureCredential());
        }

        
        public IResult Ingest(JsonElement jsonPayload)
        {
            string jsonString = jsonPayload.ToString();

            Log.Information("SerilogJsonIngested: {Payload}", jsonString);

            return Results.Ok(new { Message = "Ingested via Serilog", Timestamp = DateTime.UtcNow });
        }

        public async Task<IResult> Fetch()
        {
            try
            {
                // Serilog usually writes to 'traces' table in Log Analytics.
                // The structured data is in 'customDimensions' (or 'Properties' depending on setup), 
                // but usually the message template "SerilogJsonIngested..." is in 'message'.

                string kql = @"
                    AppTraces
                    | where Message startswith 'SerilogJsonIngested'";

                Response<LogsQueryResult> response = await _logsQueryClient.QueryWorkspaceAsync(
                    _workspaceId,
                    kql,
                    new QueryTimeRange(TimeSpan.FromDays(7)));

                var results = new List<string>();
                foreach (var row in response.Value.Table.Rows)
                {
                    var jsonString = row[4]?.ToString();
                    results.Add(jsonString);
                }

                return Results.Ok(results);
            }
            catch (Exception ex)
            {
                return Results.Problem($"Serilog Fetch Error: {ex.Message}");
            }
        }

        public async Task<IResult> DeleteLogs(string tenantId)
        {
            try
            {
                if (string.IsNullOrEmpty(tenantId))
                    return Results.BadRequest("TenantId is required for deletion.");

                // Configuration for Management API
                var subId = _configuration["ApplicationInsights:SubscriptionId"];
                var rg = _configuration["ApplicationInsights:ResourceGroupName"];
                var wsName = _configuration["ApplicationInsights:WorkspaceResourceName"];

                if (string.IsNullOrEmpty(subId) || string.IsNullOrEmpty(rg) || string.IsNullOrEmpty(wsName))
                {
                    return Results.Problem("Missing Azure Management config (SubscriptionId, ResourceGroupName, WorkspaceResourceName) in appsettings.");
                }

                // Get Management Token
                var credential = new DefaultAzureCredential();
                var tokenResult = await credential.GetTokenAsync(new Azure.Core.TokenRequestContext(new[] { "https://management.azure.com/.default" }));
                var accessToken = tokenResult.Token;

                // Construct Purge Request
                var purgeUrl = $"https://management.azure.com/subscriptions/{subId}/resourceGroups/{rg}/providers/Microsoft.OperationalInsights/workspaces/{wsName}/purge?api-version=2020-08-01";

                // We purge from 'traces' table where customDimensions contains the TenantId
                // Note: Purge filters are restrictive. This assumes 'customDimensions' can be filtered by 'contains' or similar.
                // However, deep JSON filtering in Purge is limited. 
                // A safer bet for this demo is assuming we purge by a high-level column if possible, 
                // but here we try to construct a valid body.

                var body = new
                {
                    table = "AppTraces",
                    filters = new[]
                    {
                        new { column = "OperationId", @operator = "contains", value = "00c5453f52dc68adf9ba27d35eca8f96" }
                    }
                };

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                var response = await httpClient.PostAsJsonAsync(purgeUrl, body);

                if (response.IsSuccessStatusCode)
                {
                    var operationId = response.Headers.Contains("x-ms-status-location")
                        ? response.Headers.GetValues("x-ms-status-location").FirstOrDefault()
                        : "Unknown";

                    return Results.Accepted(uri: operationId, value: new { Message = "Purge request accepted. It may take hours to complete.", OperationId = operationId });
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return Results.Problem($"Purge failed: {response.StatusCode} - {error}");
                }
            }
            catch (Exception ex)
            {
                return Results.Problem($"Delete Error: {ex.Message}");
            }
        }
    }
}
