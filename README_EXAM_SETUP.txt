REALTIME_PRO_APP - PRACTICUM STARTER
===================================

IMPORTANT
---------
This is a starter only. Read the actual exam question first.
If the exam changes Azure SQL, Event Hubs, names, fields, or the scenario, change the project to match the paper.
Do not submit placeholder values or claim a service worked if you could not prove it.

PROJECT FOLDERS
---------------
RealTimeProApp             = ASP.NET Core MVC web app
RealTimeProApp.Functions   = Azure Functions project
sql/setup.sql              = SQL table and test queries

1. COPY TO YOUR WORKING FOLDER
------------------------------
Expected location:
/Users/leonardlanga/Documents/RoseHub/RealTime_pro_app

2. FIRST LOCAL CHECK
--------------------
Web app:
cd /Users/leonardlanga/Documents/RoseHub/RealTime_pro_app/RealTimeProApp
dotnet restore
dotnet build
dotnet run

Functions:
cd /Users/leonardlanga/Documents/RoseHub/RealTime_pro_app/RealTimeProApp.Functions
dotnet restore
dotnet build
dotnet run

3. CREATE AZURE RESOURCES IN THIS ORDER
---------------------------------------
[ ] Resource Group
[ ] Azure SQL Server + Database
[ ] Run sql/setup.sql in Query Editor
[ ] App Service for the MVC app
[ ] Event Hubs namespace
[ ] Event Hub, normally order-stream unless the exam says otherwise
[ ] Function App
[ ] Monitoring / Metrics
[ ] Scaling settings if the plan supports them

Use South Africa North when available unless the exam tells you otherwise.

4. SETTINGS TO ADD TO THE WEB APP
---------------------------------
Use App Service Environment Variables or local user secrets.

Azure__EventHubConnectionString = Event Hubs connection string
Azure__EventHubName             = order-stream
Azure__SqlConnectionString      = Azure SQL connection string

For local development you can also use:
dotnet user-secrets init
dotnet user-secrets set "Azure:EventHubConnectionString" "PASTE-CONNECTION-STRING"
dotnet user-secrets set "Azure:EventHubName" "order-stream"
dotnet user-secrets set "Azure:SqlConnectionString" "PASTE-SQL-CONNECTION-STRING"

5. SETTINGS TO ADD TO THE FUNCTION APP
--------------------------------------
SqlConnectionString       = Azure SQL connection string
EventHubConnectionString  = Event Hubs connection string
EventHubName              = order-stream

AzureWebJobsStorage is created/configured by the Function App.

6. TEST THE HTTP FUNCTION FIRST
-------------------------------
Use ProcessOrderData before fighting Event Hubs.

POST JSON example:
{
  "orderId": "ORD-1001",
  "amount": 1500,
  "userId": "U001"
}

Expected status:
high-value

For amount 299, expected status:
standard

7. THEN TEST EVENT HUBS
-----------------------
Open the MVC app -> Send Event.
Send an order.
The web app publishes the event to Event Hubs.
ProcessOrderEvent receives it and writes it to Azure SQL.
Open Processed Data or run SELECT * FROM ProcessedOrders in Azure SQL Query Editor.

8. IF EVENT HUBS IS BLOCKED
---------------------------
Do not waste the whole exam.
[ ] Screenshot the restriction/error.
[ ] Keep ProcessOrderData HTTP function working.
[ ] Explain the Event Hubs production setup in the report.
[ ] Continue with SQL, testing, monitoring, scaling and theory.

9. APP SERVICE SCALING NOTES
----------------------------
Easy values to remember if the paper asks for an example:
Scale out: CPU > 70% for 5 minutes -> add 1 instance
Scale in : CPU < 30% for 10 minutes -> remove 1 instance
Minimum 1, Maximum 5
Only configure values allowed by your App Service plan.

10. LOAD TEST
-------------
Start small. Test 3-5 orders first.
If needed, use curl against the deployed HTTP Function endpoint.
Do not start a large load test before one request works.

11. SCREENSHOTS - TAKE THEM AS SOON AS THINGS WORK
---------------------------------------------------
[ ] Resource Group
[ ] App Service Overview
[ ] Live HTTPS web app
[ ] Azure SQL Database Overview
[ ] ProcessedOrders table / query results
[ ] Event Hubs namespace
[ ] Event Hub overview / partitions
[ ] Function App showing functions
[ ] ProcessOrderData successful 200 OK
[ ] Event Hub triggered Function if available
[ ] Scaling settings or plan limitation
[ ] Monitoring metrics / invocations / requests
[ ] GitHub repository

12. GIT
------
Run from the project root:
git init
git add .
git commit -m "Initial practicum real-time processing app"
git branch -M main
git remote add origin YOUR-GITHUB-URL
git push -u origin main

Then commit after major working milestones.

13. BEFORE SUBMITTING
---------------------
[ ] Student number is correct
[ ] Module code is correct
[ ] GitHub URL works
[ ] Deployed App Service URL works
[ ] Screenshots match the actual exam requirements
[ ] Theory questions answered
[ ] No connection strings or passwords appear in the document or GitHub
[ ] Final file name matches exam instructions
