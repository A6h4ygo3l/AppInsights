# Application Insights: API Key Authentication Scenario

This document explains how to implement the "Scenario 3" for ingesting and fetching logs using **Connection String + API Key**.

## 1. Ingestion (No Data Change)
**Status:** Already Supported.

*   You do **not** need to change code for ingestion.
*   The `ConnectionString` in `appsettings.json` is sufficient for the `TelemetryClient` (or Serilog Sink) to send data to Application Insights.
*   The "API Key" is **not** used for ingestion in the standard SDKs.

## 2. Fetching (Significant Change)
**Status:** Requires Code Changes.

The `Azure.Monitor.Query` SDK (used in our current `LogQueryHandler`) is designed for modern **Azure AD (workspace-based)** authentication. To use a legacy **API Key**, you must switch to the **REST API**.

### A. Configuration Updates
Add your Application ID and API Key to `appsettings.json`.
> **Note:** Generate these in Azure Portal -> Application Insights -> API Access.

```json
  "ApplicationInsights": {
    "ConnectionString": "YOUR_CONNECTION_STRING_HERE",
    "ApplicationId": "YOUR_APP_ID",  // <--- New
    "ApiKey": "YOUR_API_KEY"         // <--- New
  }
```

### B. Implementation Logic
You cannot use `LogsQueryClient`. Instead, use `HttpClient` to call the query endpoint directly.

#### Sample Code
```csharp
public async Task<IResult> SearchLogsRaw(string kqlQuery)
{
    var appId = _configuration["ApplicationInsights:ApplicationId"];
    var apiKey = _configuration["ApplicationInsights:ApiKey"];
    
    // URL Encode the KQL query
    var encodedQuery = Uri.EscapeDataString(kqlQuery);
    var url = $"https://api.applicationinsights.io/v1/apps/{appId}/query?query={encodedQuery}";

    using var client = new HttpClient();
    
    // Authenticate using the Header
    client.DefaultRequestHeaders.Add("x-api-key", apiKey);

    var response = await client.GetAsync(url);
    var jsonConfig = await response.Content.ReadAsStringAsync();
    
    // Returns raw JSON response from Azure
    return Results.Content(jsonConfig, "application/json");
}
```

## 3. Key Differences

| Feature | Workspace ID + Azure AD (Current) | Application ID + API Key (Proposed) |
| :--- | :--- | :--- |
| **Security** | **High**. Uses RBAC, Managed Identity, and token rotation. | **Low**. Static secret key. Harder to audit usage. |
| **Library** | `Azure.Monitor.Query` SDK (Type-safe, robust). | Raw `HttpClient` (Manual JSON parsing required). |
| **Setup** | Requires `az login` or Service Principal. | Requires generating a Key in Portal. |
| **Recommendation** | **Recommended** for Production services. | **Legacy**. Good for quick scripts or external tools. |
