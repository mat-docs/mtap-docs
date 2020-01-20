# ![logo](/Media/branding.png) Telemetry Analytics Data Persistence

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- **Seeding Data**<br>
  - [*Example*](#seed-data-example)<br>
- [**API**](API.md)<br>

Seeding is used to start service with a preconfigured set of streaming and topic configurations. If you wish to configure the service via API, leave ```SeedOption``` at `""`. When seeding is used it destroys any pre-existing influx writer configuration on service start, so anything configured via API will be lost. Any recorded session will remain intact.

In the [**Installation**](/docs/Installation.md) section the following properties were previously not explained:

| Property | Description |
|--|--|
| SeedOption |  The three modes supported: <br> <ul><li> "" = never seed </li> <li> "always" = always seed </li><li> "empty" = seed if database is empty </li></ul>
| SeedData | [See below](#seed-data) 

#### Seed Data
| Property | Description | 
|--|--|
| streamingConfigs | Array of streaming config. [See below](#streaming-config) |
| topicConfigs | Array of topic config. [See below](#topic-config) |

</br>

##### Streaming Config
| Property | Description | Suggested default |
|--|--|--|
| Id | The Id of the configuration. This is used to reference to the streaming configuration from topic configuration. Please note the final Ids are not guaranteed to remain the same as defined, but the references set up here will remain correct |
| name | The friendly name of the streaming config |
| description | Description for humans |
| protocol | The broker protocol to use. Valid values are `kafka` or `mqtt` |
| brokerList | Address of the message broker cluster |
| dependencyUrl | The [dependency service](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/115003531373-API-Reference-Dependencies-Service) to read dependencies from. |
| dependencyGroup | The dependency group used when reading dependencies. | "dev"
| username | The username necessary to access the broker. Relevant for `mqtt` |
| password | The password necessary to access the broker. Relevant for `mqtt` |

</br>

##### Topic Config
| Property | Description | 
|--|--|
| name | The friendly name of the topic config |
| description | Description for humans |
| sqlConnectionString | Connection string for session metadata relational database |
| eventTagGroup | The event group name used for tagging samples for contextual information |
| startAutomatically | Whether the topic should be recorded from  automatically without explicit start via API |
| streamingConfiguration | [Id of the streaming config](#streaming-config). This tells the app what broker it should contact to read from this topic.
| partitions | Defines what partitions this topic configuration should listen to. If left empty, all partitions will be read, else only specified. Partitions are defined in comma separated list. Example: `"1, 2, 3"`
| influxDbConnections | Array of influx db connections. [See below](#influx-db-connections)

<br>

##### Influx DB Connections
| Property | Description | 
|--|--|
| label | AAS sessions with this label will use this influx db connection. Useful when a single influx db is unable to handle all traffic on a single topic. `"*"` will match any session label not specified in other influx db connection within topic config.
| influxDbUrl | The address of influxDB
| database | The influx DB database name to use
| measurement | The influx DB measurement name to use

#### Seed data example
```json
{
  "streamingConfigs": [
    {
      "id": 0,
      "name": "kafka streaming",
      "description": "Streaming settings to use Kafka",
      "protocol": "Kafka",
      "brokerList": "localhost:9092",
      "dependencyUrl": "http://localhost:8180/api/dependencies/",
      "dependencyGroup": "dev",
      "username": "",
      "password": ""
    },
    {
      "id": 1,
      "name": "mqtt streaming",
      "description": "Streaming settings to use MQTT",
      "protocol": "Mqtt",
      "brokerList": "localhost",
      "dependencyUrl": "http://localhost:8180/api/dependencies/",
      "dependencyGroup": "dev",
      "username": "test",
      "password": "test"
    }
  ],
  "topicConfigs": [
    {
      "name": "TestTopic",
      "description": "TestTopic for general purpose testing with Kafka",
      "sqlConnectionString": "server=localhost\\SQLEXPRESS;Initial Catalog=Test;Integrated Security=true;",
      "eventTagGroup": "",
      "startAutomatically": true,
      "streamingConfiguration": 0,
      "influxDbConnections": [
        {
          "label": "Test",
          "influxDbUrl": "http://localhost:8086",
          "database": "TestDatabase",
          "measurement": "TestMeasurement"
        },
        {
          "label": "Race",
          "influxDbUrl": "http://localhost:8086",
          "database": "RaceDatabase",
          "measurement": "RaceMeasurement"
        },
        {
          "label": "*",
          "influxDbUrl": "http://localhost:8086",
          "database": "OtherDatabase",
          "measurement": "OtherMeasurement"
        }
      ],
      "partitions": " 0, 10, 2 "
    },
    {
      "name": "TestTopic2",
      "description": "TestTopic for general purpose testing with MQTT",
      "sqlConnectionString": "server=localhost\\SQLEXPRESS;Initial Catalog=Test;Integrated Security=true;",
      "eventTagGroup": "",
      "startAutomatically": true,
      "streamingConfiguration": 1,
      "influxDbConnections": [
        {
          "label": "*",
          "influxDbUrl": "http://localhost:8086",
          "database": "TestDatabase",
          "measurement": "TestMeasurement"
        }
      ]
    }
  ]
}
```
