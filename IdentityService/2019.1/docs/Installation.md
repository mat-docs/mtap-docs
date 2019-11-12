# ![logo](/Media/branding.png) Identity Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- **Installation**<br>
- [**API**](API.md)<br>

#### .NET Core runtime
First you need to install .NET Core 2.1 runtime. You can download it [here](https://www.microsoft.com/net/download/dotnet-core/2.1). Example for Ubuntu 18.04 LTE: 

```
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get --yes install apt-transport-https
sudo apt-get update
sudo apt-get --yes install aspnetcore-runtime-2.1
```

#### Docker

Download the docker image [here](https://bintray.com/beta/#/mclarenappliedtechnologies/mtap/identity-service?tab=readme)

#### Running Identity Service from Terminal

Alternatively, you can use Identity Service by adding the relevant configuration in `appsettings.Production.json` file and running the following command from the terminal. Make sure you are in the same directory as the **MAT.TAP.IdentityServer.dll**.

    dotnet MAT.TAP.IdentityServer.dll --urls="http://*:5000"

A sample configuration and an explanation of settings is given below.

```
{
  "ConnectionStrings": {
    "IdentityDatabase": "server=(localdb)\\MSSQLLocalDB;Initial Catalog=MAT-TAP-IdentityServer"
  },
  "InitializeDatabase": true,
  "Logging": {
    "IncludeScopes": false,
    "LogLevel": {
      "Default": "Debug",
      "System": "Information",
      "Microsoft": "Information"
    }
  },
  "OAuthServer": "http://localhost:5000",
  "TokenCleanUp": {
    "EnableCleanUp": true,
    "CleanUpInterval": 10080 // In minutes.,
  },
  "AllowedOrigins": [
    "*"
  ] // Cors Policy. List of origins or wildcard character to allow all origins.
}
```

- `OAuthServer`: Address of the Identity Service to authorize access to user management Apis. **If you are accessing the API from outside using an external IP address you may need to use that external address here.**
- `InitializeDatabase`: Set `True` to initialize database configured in ConnectionStrings section. This creates the database if it doesn't exist, applies any pending database migrations and creates the admin user and a default client.
- `ConnectionStrings:IdentityDatabase`: SQL Server connection string to Identity Service storage.
- `TokenCleanUp:EnableCleanUp`: Flag indicating whether stale tokens will be automatically cleaned up from the database.
- `TokenCleanUp:CleanUpInterval`: Token clean up interval in minutes.
- `AllowedOrigins`: Enables support for Cross-Origin Resource Sharing. You can explicilty list the origins or use asterisk symbol as wildcard to allow all origins.  