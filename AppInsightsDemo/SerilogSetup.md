# Serilog Integration Setup

This project uses **Serilog** to perform structured logging to Application Insights (Log Analytics).

## 1. NuGet Packages
We installed the following packages:
*   `Serilog.AspNetCore`: Integration with ASP.NET Core host and pipeline.
*   `Serilog.Sinks.ApplicationInsights`: The sink that sends logs content to Azure Application Insights.

## 2. Configuration (`Program.cs`)
We configured the Host to use Serilog, replacing the default logger:
```csharp
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .WriteTo.Console()
    .WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces));
```
*   `TelemetryConverter.Traces`: Maps Serilog log events to the `traces` table in Log Analytics.

## 3. How It Works

### Ingestion (`POST /api/serilog/ingest`)
When you send a JSON payload, we log it using **Structured Logging**:
```csharp
Log.Information("SerilogJsonIngested: {Payload}", jsonString);
```
*   We use `{Payload}` to store the exact JSON string.
*   In Log Analytics, this entry appears in the `traces` table.
*   The JSON payload is stored inside the `customDimensions` column under the key `Payload`.

### Retrieval (`GET /api/serilog/fetch`)
To read it back, we query the `traces` table using KQL:
```kql
traces 
| where message startswith 'SerilogJsonIngested'
| extend JsonData = parse_json(tostring(customDimensions.Payload))
```
This extracts the original JSON object from the `customDimensions` column, allowing field-based filtering (e.g., `JsonData.TenantId`).

## 4. Usage
**Ingest**:
`POST /api/serilog/ingest` with raw JSON body.

**Fetch**:
`GET /api/serilog/fetch?tenantId=...` to search logs ingested via this new Serilog flow.
