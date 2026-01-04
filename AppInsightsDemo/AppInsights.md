# AppInsights 

## Steps to create app
```
dotnet new webapi -n AppInsightsDemo
cd AppInsightsDemo/
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

## Sample Jsons to Insert
```
{
  "deviceId": "D-123",
  "temperature": 25.5,
  "status": "active"
}
=========
{
  "user": {
    "id": 101,
    "name": "Alice"
  },
  "action": "login",
  "timestamp": "2025-01-04T12:00:00Z"
}
=========
[
  { "id": 1, "value": "A" },
  { "id": 2, "value": "B" }
]
=========
```

## Queries 
Find events with temperature filter
```
customEvents
| where name == "JsonIngested"
| extend payloadObj = parse_json(tostring(customDimensions.Payload))
| where toint(payloadObj.temperature) < 30
| project timestamp, payloadObj
```

Get the last 10 ingested events.
```
customEvents
| where name == "JsonIngested"
| take 10
```

Count number of events per device.
```kusto
customEvents
| where name == "JsonIngested"
| extend payload = parse_json(customDimensions.Payload)
| summarize EventCount = count() by tostring(payload.deviceId)
```

Average temperature over time.
```
customEvents
| where name == "JsonIngested"
| extend payload = parse_json(customDimensions.Payload)
| make-series AvgTemp = avg(todouble(payload.temperature)) on timestamp from ago(24h) to now() step 1h
| render timechart
```
