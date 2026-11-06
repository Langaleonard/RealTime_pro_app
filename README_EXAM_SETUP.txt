REALTIME AGRICULTURE PRACTICUM - QUICK SETUP
============================================

WHAT THIS STARTER ALREADY DOES
- Farmer demo login
- Capture soil telemetry manually
- Upload a drone image
- Mark moisture below 30% as "Irrigation Required"
- Save telemetry to Azure Table Storage when connected
- Save drone images to Blob container: drone-images
- Create irrigation messages in Queue: irrigation-commands
- Create operational logs in Azure File Share: farm-logs
- If Azure is not connected, the same screens still work with a local in-memory fallback

RUN THE WEB APP
cd /Users/leonardlanga/Documents/RoseHub/RealTime_pro_app
dotnet restore
dotnet build
dotnet run

CONNECT AZURE STORAGE
1. Azure Portal > Storage accounts > create/select your practicum storage account.
2. Go to Access keys > copy a Connection string.
3. For LOCAL testing, use .NET user secrets. From the project root run:
   dotnet user-secrets set "AzureStorage:ConnectionString" "PASTE_CONNECTION_STRING_HERE"
4. Do not put the real connection string in appsettings.json or GitHub.
5. The key the app expects is:
   AzureStorage:ConnectionString
5. For Azure App Service, add Environment Variable:
   AzureStorage__ConnectionString = <connection string>

AZURE RESOURCES CREATED AUTOMATICALLY BY THE APP
- Table: Telemetry
- Blob container: drone-images
- Queue: irrigation-commands
- File share: farm-logs

SIMPLE TEST
1. Open Capture.
2. Enter Sensor ID SENSOR-001 and Field A.
3. Set Soil Moisture to 21 and Temperature to 29.
4. Add a short drone note and optionally upload an image.
5. Submit.
6. Processed Data should show "Irrigation Required".
7. Logs / Commands should show a START_IRRIGATION message.
8. In Azure, screenshot the Telemetry table, drone-images container, irrigation-commands queue and farm-logs file share.

EVENT HUBS / AZURE FUNCTIONS FOR THE EXAM
This working web demo uses Azure Storage directly so it stays reliable.
If the exam specifically requires Event Hubs, use this architecture:
Sensor/Web App -> Event Hubs -> Azure Function -> processed storage/database -> queue command

If Event Hubs is allowed:
- Create Event Hubs namespace.
- Create event hub named soil-telemetry.
- Add its connection string to the Function App settings.
- Use an Event Hubs-triggered Function to apply the same moisture < 30 logic.

If Azure SQL is specifically required by the paper, follow the paper and add Azure SQL for processed records. Do not replace a required service just because this starter has a working Azure Table fallback.

IMPORTANT
- The login is intentionally a simple practicum demo, not production authentication.
- Do not commit connection strings, local.settings.json or passwords.
- Follow the actual exam question first. This starter is only a fast base.


RUN THE FUNCTIONS PROJECT (OPTIONAL PRACTICUM DEMO)
cd /Users/leonardlanga/Documents/RoseHub/RealTime_pro_app/RealTimeProApp.Functions
cp local.settings.example.json local.settings.json
# Replace the placeholder connection string in local.settings.json, then:
dotnet restore
dotnet build
dotnet run

Test the HTTP function with:
curl -X POST http://localhost:7009/api/ProcessTelemetry \
  -H "Content-Type: application/json" \
  -d '{"sensorId":"SENSOR-001","fieldName":"Field A","soilMoisture":21,"temperature":29}'
