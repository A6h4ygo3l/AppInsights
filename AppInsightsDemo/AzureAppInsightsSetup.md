# How to Setup Azure Application Insights

Follow these steps to create an Application Insights resource in Azure and get the connection string.

## 1. Create Application Insights Resource

1.  Log in to the [Azure Portal](https://portal.azure.com/).
2.  Search for **Application Insights** in the search bar and select it.
3.  Click **+ Create** (or **+ New**).
4.  **Basics Tab**:
    *   **Subscription**: Select your Azure subscription.
    *   **Resource Group**: Select an existing group or create a new one (e.g., `rg-app-insights`).
    *   **Name**: Enter a unique name (e.g., `my-app-insights-service`).
    *   **Region**: Select a region close to your users (e.g., `East US`).
    *   **Resource Mode**: Select **Workspace-based** (Recommended).
    *   **Log Analytics Workspace**: Select an existing one or create a new one. This is where the data is actually stored.
5.  Click **Review + create**.
6.  Click **Create** and wait for the deployment to finish.

## 2. Get Connection String

1.  Once the resource is created, click **Go to resource**.
2.  In the **Overview** blade, look for the **Connection String** field on the top right (or middle) of the essentials section.
3.  Copy the connection string. It looks like:
    `InstrumentationKey=xxxx-xxxx-xxxx;IngestionEndpoint=https://...;LiveEndpoint=https://...`
4.  Paste this connection string into your `appsettings.json` file in the `JsonIngestionService` project:
    ```json
    "ApplicationInsights": {
      "ConnectionString": "YOUR_CONNECTION_STRING_HERE"
    }
    ```

## 3. Verify Data (Log Analytics)

1.  After sending data to the service, wait a few minutes (up to 5 mins).
2.  In your Application Insights resource, go to **Monitoring** -> **Logs** in the left menu.
3.  In the query editor, type:
    ```kusto
    customEvents
    ```
    or
    ```kusto
    traces
    ```
4.  Click **Run** to see your ingested data.
