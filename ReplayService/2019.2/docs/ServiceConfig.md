# ![logo](/Media/branding.png) Replay Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- **Service configuration**<br>
- [**Authorization**](Authorization.md)<br>
- [**API**](API.md)<br>

# Service Configuration
[dependency service]: https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/115003531373-API-Reference-Dependencies-Service
[protobuf]: https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/360008375233-Protobuf-Extension
[TAP API]: /TapApi/README.md
[Identity Service]: /IdentityService/README.md
[cors]: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS


The configuration file of the service is called ```appsettings.json```
<details>
<summary>Example settings</summary>

```json
{
  "Server": {
    "ListenAddress": "http://*",
    "ListenPort": 5001,
    "CorsOrigins": ["*"]
  },
  "Kafka": {
    "DependencyService": "http://dependency-service/api/dependencies/",
    "BrokerList": "127.0.0.1",
    "UseProtobuf": true
  },
  "TapApi": {
    "Url": "http://tap-api-service",
    "User": "username",
    "Password": "password"
  },
  "OAuth": {
    "Enabled": true,
    "Url": "http://identity-server"
  }
}
```
</details>

#### Server properties

| Property  | Description                                                 | Default value | Example                                                                   |  
|----------------|-------------------------------------------------------------|---------------|---------------------------------------------------------------------------|
| ListenAddress       | The url the service will listen on. Use "*" to indicate that the server should listen for request on any IP Address or hostname using the specified `ListenPort` and protocol. The protocol (http:// or https://) must be included.                                           |     "http://*"       | https://replay-service                                                                        |
| ListenPort        | The port the server should listen to for requests made to `ListenAddress`.   |       5000        |   Any free port between 0 to 65535                                                    |  
| CorsOrigins         | List of addresses enabled for [cors]. Use "*" if you wish to allow all requests                |      ["*"]         | ["http://tap-api/explorer", "https://some-other-service"] |

<br>

#### Kafka properties

| Property  | Description                                                 | Default value | Example                                                                   |  
|-|-|-|-|
| DependencyService       |  The [dependency service] to publish dependencies to when replaying a session     |           |    "http://dependency-service/api/dependencies/"          |
| BrokerList        | Address of the message broker cluster. Multiple can be specified using comma  |               |    "127.0.0.1"       |  
| UseProtobuf         | True if [protobuf] should be used, false otherwise. If not enabled, message encoding will be JSON |   false      | true or false |

<br>

#### Tap Api properties

| Property  | Description | Example |  
|-|-|-|
| Url       |  [TAP API] service address   |       "http://tap-api-service"          |
| User      | The user to authenticate with when requesting data from TAP API. See `OAuth` section.  |              "replayuser" |  
| Password   | The password for the specified `User`. | "myp@ssw0rd" |

<br>


#### OAuth properties

| Property  | Description | Example |  
|-|-|-|
| Enabled       | True if OAuth should be enabled. Suggested OAuth is [Identity Service]. If disabled then Replay Service endpoints do not require authentication.             |    true or false          |
| Url        | Th OAuth service provider adddress.               |     "http://identity-server"      |  


