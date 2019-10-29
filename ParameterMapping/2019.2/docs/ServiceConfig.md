# ![logo](/Branding/branding.png)  Parameter Mapping Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- **Service configuration**<br>

# Service Configuration
[Identity Server]: https://github.com/McLarenAppliedTechnologies/mat.tap.query.api/blob/master/docs/IdentityServer.md
[cors]: https://developer.mozilla.org/en-US/docs/Web/HTTP/CORS
[protobuf]: https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/360008375233-Protobuf-Extension

The configuration file of the service is called ```appsettings.json```
<details>
<summary>Example settings</summary>

```
{
  "Kafka": {
    "BrokerList": "localhost:9092",
    "ConsumerGroup": "MAT.TAP.ParameterMapping",
    "DependencyService": "http://localhost:8081/api/dependencies/",
    "UseProtobuf": false
  },
  "InputTopics": [ "input_topic" ],
  "OutputTopics": [ "output_topic" ],
  "ParameterMappings": [
    {
      "SourceIdentifier": "carSpeed",
      "TargetIdentifier":  "vCar:Chassis" 
    }
  ]
}
```

</details>

#### Properties

| Property  | Description | Example |  
|-|-|-|
| Kafka | The Kafka broker configuration. [See below](#kafka-properties).
| InputTopics | An array of topic names to listen to for live data. Linked to `OutputTopics` by index. | `[ "Topic1", "Topic2" ]` |
| OutputTopics | An array of topic names to write the renamed parameteres to. Linked to `InputTopics` by index. | `[ "Topic1", "Topic2" ]` |
| ParameterMappingSettings        | The parameter mapping configuration. See Below |  

<br>

#### Kafka properties

| Property  | Description                                                 | Default value | Example                                                                   |  
|-|-|-|-|
| BrokerList        | Address of the message broker cluster. Multiple can be specified using comma  |               |    `"127.0.0.1"`       |  
| ConsumerGroup         | The identity to use when reading from Kafka. | `SignalR_production` | |
| DependencyService       |  The [dependency service] to publish dependencies to when replaying a session     |           |    `"http://dependency-service/api/dependencies/"`          |
| UseProtobuf         | True if [protobuf] should be used, false otherwise. If not enabled, message encoding will be JSON |   `false`      | `true` or `false` |

<br>

#### ParameterMappings array
Contains a listo of source to target parameter name mappings.
| Property  | Description |  
|-|-|
| SourceIdentifier   |  The parameter identifier to rename in the source session |
| TargetIdentifier   |  The parameter identifier to rename to in the output session |