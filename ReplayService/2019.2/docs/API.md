# ![logo](/Branding/branding.png) Replay Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Service configuration**](ServiceConfig.md)<br>
- [**Authorization**](Authorization.md)<br>
- **API**<br>

[TAP API]: https://github.com/McLarenAppliedTechnologies/mat.tap.query.api
[connection]: https://github.com/McLarenAppliedTechnologies/mat.tap.query.api/blob/master/docs/GettingStarted.md#influxdb-connections
[service configuration]: /docs/ServiceConfig.md

Influx Writer API is documented using Open API (formerly Swagger) and is integrated with Swagger UI. UI is available at `<your-replay-sevice-addresss>/swagger/index.html`. Example: `http://localhost:8183/swagger/index.html`. Refer to that for specific endpoints, models. 

Depending on the version of Swagger UI, it might look slightly different, but when you visit it you should be presented with a list of endpoints. Each endpoint will provide you with example model as required. Explanation for each property of the model will also be provided. Swagger UI allows for trying out the endpoint in the browser. If any authentication is necessary, it can be usually set above the first listed endpoint on the right.

## Flow of the API
1. If OAuth is enabled in [service configuration](/docs/ServiceConfig.md) then to access the API a [token must be acquired](/docs/Authorization.md)
2. Create a ReplayConfiguration
3. Start/Stop ReplayConfiguration if you haven't specified it to automatically start in step 2

## Replay Configuration Model

Due to the complexity of the model, it is explained here with more details than Swagger UI. A replay configuration is a set of instructions for the API to describe where the replay session can be found, what part of it should be replayed and to what broker topic. An [example can be found below](#example-replay-configuration).

| Property  | Description  | Default value | Valid Values |  
|--|--|--|--|
| source | Source model describing what to play back. [See below](#source).
| output | Output model describing the where and how to write the session. [See below](#output).
| autostart | True if the session should be replayed when configuration is posted // updated
| replaySpeed | Speed multiplier to use when replaying session. The higher the number the quicker the playback is. <br> Actual playback speed depends on network speed and processing power. <br> Examples: 1 = normal, 0.5 half speed, 2 = double speed. 0 = as fast as possible. | `1` | 0-100
| replayInLoop | Whether the session should be replayed from start once it completed.
| dataSelection | The parameter data selected from the source session into the output session. [See below](#dataselection).
</br>

#### Source
Source describes the session to replay.

| Property  | Description  | Default value | Valid Values |  
|--|--|--|--|
| connectionId       | The [connection] to use from the [TAP API] configured in th replay [service configuration] | | Any valid connection|
| sessionId | The session to replay. | | Any session available in the connection
| version   | The version of the session to replay. If not specified or null then latest version will be used, else the specified version | | null for latest, >= 0 for specific |
| startTime | Session will be replayed starting from this time. The value is nanoseconds since 1/1/1970. If value provided is less than the start of session then will use the start of session. | `0` (defaults to start of session)
| endTime | Session will be replayed up to this time. The value is nanoseconds since 1/1/1970. If value provided is greater than end of session then will use end of session | `7258122000000000000` (01/01/2200, defaults to end of session)
| laps | Array of Integers. Only sections of the session where the lap number is among the provided will be replayed. If none provided, filter is ignored. | `[]`

</br>

#### Output

| Property  | Description  | Default value | Valid Values |  
|--|--|--|--|
| topic | The broker topic to use for the resulting session| | Any
| messageInterval | The max time difference between samples within TData, TSamples messages in milliseconds. For example, message will be split to two if sample times are 0, 10, 30, and interval is 20. | `200` | 20 or more
| indentifier | The friendly name of the output session. See below for [identifier placeholders](#identifier-placeholders). | `{{identifier}} replay {{time yyMMddHHmmss}}` | Any string
| version | Versioning number of the resulting session. Mainly used when `inheritSourceId` is also specified as true. Used only when set to greater than 0 | `0` | null, 0 or more
| inheritSourceId | Whether the resulting session should have the same SessionId as the source session. More info can be found regarding [versioning in TAP API](https://github.com/McLarenAppliedTechnologies/mat.tap.query.api/blob/master/docs/SessionVersions.md) documentation. | `false` | true or false

<br>

##### Identifier placeholders

| Placeholder  | Description  | Example |
|--|--|--|
| {{identifier}} | Replaced by the source session's identifier (friendly name)
| {{time timeformat}} | Replaced by the time at the start of replay with the [format specified](https://docs.microsoft.com/en-us/dotnet/standard/base-types/custom-date-and-time-format-strings). | If current ISO 8601 Time is 2019-05-12T18:23:05 <br> and format used is  {{time yyMMddHHmmss}} <br> then value would be 190512182305


</br>

#### Data Selection
Data selection describes the feed composition of the replayed session. It is a dictionary of feed name and [feed defitinion](#feed-definition). See [example below](#example-replay-configuration).

#### Feed Definition


| Property  | Description 
|--|--|
| parameters | An array of parameter definitions to include in this feed |
| frequency | Samples will be output at this feed frequency. Frequency is defined in hz. **If 0 is defined, then parameters will be output at their native frequency** |

#### Parameter Definition

| Property  | Description 
|--|--|
| identifier | Identifier of the parameter to include in this feed |
| aggregate | The aggretation to use for the parameter when the feed's frequency is greater than 0. Valid values are `first`, `avg`, `min`, `max` |

#### Example replay configuration

```json
{
  "source": {
    "connectionId": "TAP API Connection",
    "sessionId": "SessionId",
    "version": 0,
    "startTime": 0,
    "endTime": 7258122000000000000,
    "laps": [1, 2, 3]
  },
  "output": {
    "topic": "TestTopic",
    "messageInterval": 200,
    "identifier": "{{identifier}} replay {{time yyMMddHHmmss}}",
    "version": 0,
    "inheritSourceId": false
  },
  "autoStart": true,
  "replaySpeed": 1,
  "replayInLoop": false,
  "dataSelection": {
    "feedOne": {
      "parameters": [
        {
          "identifier": "vCar:Chassis",
          "aggregate": "avg"
        }
      ],
      "frequency": 0
    },
    "feedTwo": {
      "parameters": [
        {
          "identifier": "gLat:Chassis",
          "aggregate": "avg"
        },
        {
          "identifier": "gLong:Chassis",
          "aggregate": "avg"
        }
      ],
      "frequency": 100
    }
  }
}
```