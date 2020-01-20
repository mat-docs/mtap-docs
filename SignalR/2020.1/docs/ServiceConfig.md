# ![logo](/Media/branding.png) SignalR Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- **Service configuration**<br>
- [**API**](API.md)<br>

# Service Configuration
[Identity Service]: /IdentityService/README.md
[cors]: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS


The configuration file of the service is called ```appsettings.json```
<details>
<summary>Example settings</summary>

```
{
  "Server": {
    "ListenAddress": "http://*",
    "ListenPort": "8880",
    "CorsOrigins": [ "*" ]
  },

  "TopicNames": [
    "TestTopic"
  ],

  "Kafka": {
    "DependencyService": "https://localhost:8180/api/dependencies/",
    "BrokerList": "localhost:9096",
    "ConsumerGroup": "SignalR_production"
  },
 
  "OAuthServer": "http://192.168.56.101:5000",
  
  "Licensing": {
    "CustomerId": "00000000-0000-0000-0000-000000000000",
    "LicenseServerUri": "https://localhost/mockSentinelEMS"
  }
}
```

</details>

#### Properties

| Property  | Description | Example |  
|-|-|-|
| Server | The API server configuration. [See below](#server-properties). | |
| TopicNames | An array of topic names to listen to for live data | `[ "Topic1", "Topic2" ]` |
| Kafka | The Kafka broker configuration. [See below](#kafka-properties).
| OAuthServer        | The OAuth service provider adddress. TAP comes with [Identity Service] as OAuth provider.              |     `"http://identity-server"`      |  


#### Server properties

| Property  | Description                                                 | Default value | Example                                                                   |  
|----------------|-------------------------------------------------------------|---------------|---------------------------------------------------------------------------|
| ListenAddress       | The url the service will listen on. Use "*" to indicate that the server should listen for request on any IP Address or hostname using the specified `ListenPort` and protocol. The protocol (http:// or https://) must be included.                                           |     `"http://*"`       | `https://replay-service`                                                                        |
| ListenPort        | The port the server should listen to for requests made to `ListenAddress`.   |       `5000`        |   Any free port between 0 to 65535                                                    |  
| CorsOrigins         | List of addresses enabled for [cors]. Use "*" if you wish to allow all requests                |      ["*"]         | `["http://tap-api/explorer", "https://some-other-service"]` |

<br>

#### Kafka properties

| Property  | Description                                                 | Default value | Example                                                                   |  
|-|-|-|-|
| DependencyService       |  The [dependency service] to publish dependencies to when replaying a session     |           |    `"http://dependency-service/api/dependencies/"`          |
| BrokerList        | Address of the message broker cluster. Multiple can be specified using comma  |               |    `"127.0.0.1"`       |  
| ConsumerGroup         | The identity to use when reading from Kafka. | `SignalR_production` | |


#### Licensing properties
| Property | Description | Example |
|--|--|--|
| CustomerId| Unique id for every customer. It is used to identify if the customer is licensed to use this application. |  |
| LicenseServerUri | License server URI, provided to the customer upfront. It is used to host the licensing service for customer license management and validation. | https://localhost/mockSentinelEMS |
