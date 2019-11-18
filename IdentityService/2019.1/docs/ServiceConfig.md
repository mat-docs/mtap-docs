# ![logo](/Media/branding.png) Identity Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](docs/Installation.md)<br>
- **Service Config**<br>
- [**API**](docs/API.md)<br>

# Service Configuration

The configuration file of the service is called ```appsettings.Production.json```
<details>
<summary>Example settings</summary>

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
</details>


- `ConnectionStrings.IdentityDatabase`: SQL Server connection string to Identity Service storage.
- - `InitializeDatabase`: Set `True` to initialize database configured in ConnectionStrings section. This creates the database if it doesn't exist, applies any pending database migrations and creates the admin user and a default client.
- `OAuthServer`: Address of the Identity Service to authorize access to user management Apis. **If you are accessing the API from outside using an external IP address you may need to use that external address here.**
- `TokenCleanUp.EnableCleanUp`: Flag indicating whether stale tokens will be automatically cleaned up from the database.
- `TokenCleanUp.CleanUpInterval`: Token clean up interval in minutes.
- `AllowedOrigins`: Enables support for Cross-Origin Resource Sharing. You can explicilty list the origins or use asterisk symbol as wildcard to allow all origins. 