# AppInsights
Monitoring the app and redirects data to log analytics.

**Azure Application Insights** is a feature of Azure Monitor that provides extensible Application Performance Management (APM) for developers and DevOps professionals. It works by installing a SDK (i.e. small instrumentation package) in the .Net application, which monitors the app and directs telemetry data to an Azure Log Analytics workspace.

## Covered Areas 
- Connected with (Connection String + API Key)
- Connected With (Workspace ID + Azure AD)
- Connected With (Serilog + Connection String)

## Usage
It is primarily used for:
*   **Live Monitoring**: Automatically detecting performance anomalies.
*   **Diagnostics**: Help to diagnose issues like failed requests, exceptions, or slow dependencies.
*   **Usage Analytics**: Understanding what users do with the app (e.g., page views, custom events).
*   **Telemetry Storage**: Storing logs, metrics, and traces for querying and visualization.

## Benefits as a "Database" for Telemetry
While not a traditional transactional database (like SQL), it is excellent for event data:
1.  **Schema-less Ingestion**: Perfect for storing semi-structured JSON logs via custom dimensions.
2.  **High Ingestion Rate**: Designed to handle massive amounts of telemetry data without throttling your app.
3.  **Powerful Query Language (KQL)**: Kusto Query Language allows for incredibly fast searching, filtering, and aggregating of millions of records.
4.  **Retention Policies**: Built-in lifecycle management to automatically delete old data (e.g., keep logs for 30-90 days).
5.  **Integration**: Native integration with Azure Dashboards, Alerts, and Power BI.
