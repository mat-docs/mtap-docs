# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- [**Introduction**](../README.md)<br>
- **Installation**<br>
  - [Influx TAP API](#influx)
  - [SQLRace TAP API](#sqlrace)
- [**Getting started**](GettingStarted.md)<br>
- [**Authorization**](Authorization.md)<br>
- [**Querying Metadata**](Metadata.md)<br>
- [**Consuming Data**](ConsumingData.md)<br>
- [**Consuming Events**](ConsumingEvents.md)<br>
- [**Session Versions**](SessionVersions.md)<br>

# Installation

The TAP API provides a micro service per persitence mechanism (Influx and SQLRace)

## Influx
#### Install the Identity Service
The [**Identity Service**](/IdentityService/README.md) must be installed before installing MAT.TAP.TelemetryAnalytics.API.

#### Deploy the Docker Container
For production usage we recommend using the Docker containers downloadable here:
https://bintray.com/beta/#/mclarenappliedtechnologies/mtap/tapi-influx-service?tab=readme (requires login to bintray)

#### Basic usage
The API can be started from a console in Windows or Linux. This is useful for initial experimentation and familiarisation.

A zip file is included in the release bundle for MTAP. Locate the `TAPI Influx` zip, and expand it to a folder of your choice.
Edit the `appsettings.Production.json` config file.

Configure the connection string, example:
```
{
  "ConnectionStrings": {
    "Model": "server=.\\SQLEXPRESS;Initial Catalog=Telemetry.Analytics.API.Influx;User Id=test;Password=test;",
  },
  "InitializeDatabase": true,
  "OAuthServer": "http://localhost:5000"
}
```
Where:
 - SQLExpress refers to the SQLServer DB that holds the TAP API configuration.
 - OAuthServer refers to your instance of the Identity Service. **If you are accessing the API from outside using an external IP adress you may need to use that external address here.**

Set  `InitializeDatabase` to `True` to initialize database configured in ConnectionStrings section. This creates the database if it doesn't exist and applies any pending database migrations.

Start the service from the command line as follows:
    ```dotnet MAT.TAP.AAS.TelemetryAnalytics.API.dll --urls="http://*:5000"```

Where:
 - --urls refers to the port the API is available on

## SQLRace

### Prerequisites:
- SQLRace database installed
- SQLRace license installed on machine

Run msi installer and follow instructions. **Note that API will clash with Atlas 10 and SQLRace so it has to be deployed to a standalone machine.**
