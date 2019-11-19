# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Getting started**](GettingStarted.md)<br>
- [**Authorization**](Authorization.md)<br>
- **Querying Metadata**<br>
- [**Consuming Data**](ConsumingData.md)<br>
- [**Session Versions**](SessionVersions.md)<br>


## Querying Metadata

This section explains in detail what endpoints are available to retrieve session metadata and how to execute them.
It is possible to collect session metadata from both InfluxDb and SqlRace data storages. Collected data is provided in Json format.

Sessions
========

The ```/sessions``` endpoint gives access to a list of sessions available for a given connection. Since there may be multiple versions of sessions with the same session id, only the latest version of a session is returned. More information on exploring session versions are described in [Session Versions](SessionVersions.md).

### Query all available sessions

Endpoint
```
GET api/v1/connections/{connection name}/sessions
```

Example  
```
GET api/v1/connections/Connection/sessions
```

Result  
```json
[
  {
    "id": "0151d834-7a23-46c6-a3fc-eb536adcf93b",
    "streamId": "0121d634-7l22-35r6-a3fc-eb536adcf93b",
    "identifier": "Identifier7",
    "timeOfRecording": "2018-12-09T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:33:08.3917995Z",
    "end": "2019-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName7",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "01bdac9f-18d9-4e8c-956b-c397206bf5a4",
    "streamId": "08fb78fa-6547-4a60-8a58-f3d6c1dd2978",
    "identifier": "Identifier5",
    "timeOfRecording": "2018-11-29T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:36:42.1170779Z",
    "end": "2019-03-28T14:36:42.1170779Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName5",
    "group": "Group10",
    "version": 1,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "09d6802c-a9b8-4a77-bc40-3dbffd88ac7b",
    "streamId": "65fd2589-9359-49d6-bc1c-5a69128358e3",
    "identifier": "Identifier4",
    "timeOfRecording": "2018-11-23T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:36:42.1170779Z",
    "end": "2019-03-28T14:36:42.1170779Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName4",
    "group": "Group10",
    "version": 3,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "0fa13add-4c1f-4dda-a7be-32bbfc1b8fc6",
    "streamId": "1e8838ca-aa7e-490c-923e-a168d1285e6a",
    "identifier": "Identifier3",
    "timeOfRecording": "2018-12-05T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:42:14.1820662Z",
    "end": "2019-03-28T14:42:14.1820662Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName3",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "150cc8cc-ef42-4730-826f-3705af91360c",
    "streamId": "0121d634-7l22-35r6-a3fc-eb536adcf93b",
    "identifier": "Identifier2",
    "timeOfRecording": "2018-12-04T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:42:14.1820662Z",
    "end": "2019-03-28T14:56:26.1820662Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName2",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  }
]
```

Optional parameters  
-------------------  

The ```/sessions``` endpoint provides a set of optional parameters from which is possible to filter, sample size it or order the set of requested data.
  
| Parameter name | Description                                                 | Default value | Example                                                                   |  
|----------------|-------------------------------------------------------------|---------------|---------------------------------------------------------------------------|
| page           | Index of the page returned in result (0 is first page)      |               | 3                                                                         |  
| pageSize       | Size of one page.                                           | 200           | 50                                                                        |
| items          | Filters sessions by session details like driver, car etc.   |               | Driver:KHA,Car:P1GTR                                                      |  
| filter         | It allows filtering on the results.                         |               | state;eq;Closed                                                           |  
| order          | It allows ordering of the results.                          |               | timeofrecording:desc                                                      |


Filters
------ 

It is possible to collect session data applying specific filters to it.<br />
There are multiple types of filters being those:

#### Filter types

| Shortcut | Full name             |
|----------|-----------------------|
| eq       | Equal                 |
| gt       | Greater than          |
| ge       | Greater than or equal |
| lt       | Less than             |
| le       | Less than or equal    |
| ne       | Not equal             |
  

#### Property filter construction

Structure of one parameter filter is:

Property filter url mask
```
{property};{filterOperationShortcut};{value}
```
  
#### Filtering by closed state

Endpoint
```
GET api/v1/connections/{connection name}/sessions
```

Example  
```
GET api/v1/connections/Connection/sessions?filter=state;eq;Closed
```

Result  
```json
[
  {
    "id": "16881d4c-7bcc-48f1-934f-a32645fc829f",
    "streamId": "0121d634-7l22-35r6-a3fc-eb536adcf93b",
    "identifier": "Identifier6",
    "timeOfRecording": "2018-12-09T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T12:36:10.6907316Z",
    "end": "2019-03-28T12:36:10.6907316Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName6",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "23d61829-cd8d-4522-8951-f9c0f3867548",
    "streamId": "08fb78fa-6547-4a60-8a58-f3d6c1dd2978",
    "identifier": "Identifier5",
    "timeOfRecording": "2018-12-10T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T12:36:10.6907316Z",
    "end": "2019-03-28T12:36:10.6907316Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName5",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "7a3eb574-2038-4fa8-bebe-9b13eef64ab7",
    "streamId": "65fd2589-9359-49d6-bc1c-5a69128358e3",
    "identifier": "Identifier4",
    "timeOfRecording": "2018-11-29T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:33:08.3917995Z",
    "end": "2019-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName4",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "7a5e7a00-1860-449f-913b-e03688223622",
    "streamId": "65fd2589-9359-49d6-bc1c-5a69128358e3",
    "identifier": "Identifier10",
    "timeOfRecording": "2018-12-02T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T12:36:10.6907316Z",
    "end": "2019-03-28T12:36:10.6907316Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName10",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "8b420497-c312-45c5-93cd-4a2758d28e66",
    "streamId": "0121d634-7l22-35r6-a3fc-eb536adcf93b",
    "identifier": "Identifier10",
    "timeOfRecording": "2018-12-10T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:33:08.3917995Z",
    "end": "2019-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName10",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  }
]
```

#### Filtering by time of recording bigger than

Endpoint
```
GET api/v1/connections/{connection name}/sessions
```

Example  
```
GET api/v1/connections/Connection/sessions?filter=timeOfRecording;gt;2018-12-03T00:00:00Z
```

Result  
```json
[
  {
    "id": "0151d834-7a23-46c6-a3fc-eb536adcf93b",
    "streamId": "0121d634-7l22-35r6-a3fc-eb536adcf93b",
    "identifier": "Identifier7",
    "timeOfRecording": "2018-12-09T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:33:08.3917995Z",
    "end": "2019-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName7",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "16881d4c-7bcc-48f1-934f-a32645fc829f",
    "streamId": "65fd2589-9359-49d6-bc1c-5a69128358e3",
    "identifier": "Identifier6",
    "timeOfRecording": "2018-12-09T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T12:36:10.6907316Z",
    "end": "2019-03-28T12:36:10.6907316Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName6",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "23d61829-cd8d-4522-8951-f9c0f3867548",
    "streamId": "65fd2589-9359-49d6-bc1c-5a69128358e3",
    "identifier": "Identifier5",
    "timeOfRecording": "2018-12-10T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T12:36:10.6907316Z",
    "end": "2019-03-28T12:36:10.6907316Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName5",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "52930321-f71b-4380-a886-45a8fe077e29",
    "streamId": "08fb78fa-6547-4a60-8a58-f3d6c1dd2978",
    "identifier": "Identifier6",
    "timeOfRecording": "2018-12-05T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:36:42.1170779Z",
    "end": "2019-03-28T14:36:42.1170779Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName6",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "8b420497-c312-45c5-93cd-4a2758d28e66",
    "streamId": "1e8838ca-aa7e-490c-923e-a168d1285e6a",
    "identifier": "Identifier10",
    "timeOfRecording": "2018-12-10T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:33:08.3917995Z",
    "end": "2019-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName10",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  }
]
```

Ordering
------

To a collection of data is also provided with the option of ordering the requested data set. The properties can be ordered by asc or desc.

#### Property order construction

Structure of one property filter is:

Property filter url mask
```
{property};{order}
```

#### Ordering by end time descending

Endpoint
```
GET api/v1/connections/{connection name}/sessions
```

Example  
```
GET api/v1/connections/Connection/sessions?orderBy=end:desc
```

Result  
```json
[
  {
    "id": "f75f1c7d-9192-42c8-89f0-c1f1b613adcc",
    "streamId": "1e8838ca-aa7e-490c-923e-a168d1285e6a",
    "identifier": "Identifier2",
    "timeOfRecording": "2018-12-02T00:00:00Z",
    "timeZone": null,
    "sessionType": "StreamingSession",
    "start": "2019-03-28T11:01:31.7641885Z",
    "end": "2019-03-28T11:15:43.7641885Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName2",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "c64c8e95-7540-4951-8815-c908e51b2491",
    "streamId": "670c3d9c-7401-444b-829d-2e0226c71aca",
    "identifier": "Identifier2",
    "timeOfRecording": "2018-12-07T00:00:00Z",
    "timeZone": null,
    "sessionType": "StreamingSession",
    "start": "2019-03-28T10:59:57.401443Z",
    "end": "2019-03-28T11:14:09.401443Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName2",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "b52b6bac-474b-4e8c-b2cd-bae44c35e71c",
    "streamId": "1e8838ca-aa7e-490c-923e-a168d1285e6a",
    "identifier": "Identifier2",
    "timeOfRecording": "2018-12-05T00:00:00Z",
    "timeZone": null,
    "sessionType": "StreamingSession",
    "start": "2019-03-28T10:51:36.6730065Z",
    "end": "2019-03-28T11:05:48.6730065Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName2",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "test-session-id",
    "streamId": "27539db2-23bb-42e8-b50b-a2b042a8f203",
    "identifier": "TestSession1",
    "timeOfRecording": "2018-12-03T00:00:00Z",
    "timeZone": null,
    "sessionType": "StreamingSession",
    "start": "2019-03-28T10:51:36.6730065Z",
    "end": "2019-03-28T11:05:48.6730065Z",
    "lapsCount": 10,
    "state": "Open",
    "topicName": "TopicName1",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "0179b921-1d0d-4b9e-96e1-9e9f1d86ccfd",
    "streamId": "24dc48bd-e0d9-4b29-9db9-e1d6301aaf7c",
    "identifier": "Identifier10",
    "timeOfRecording": "2018-12-12T00:00:00Z",
    "timeZone": null,
    "sessionType": "StreamingSession",
    "start": "2019-03-28T11:01:31.7641885Z",
    "end": "2019-03-28T11:01:31.7641885Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName10",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  }
]
```
<br />

### Query a specific session by its identifier

The API allows to retrieve a session by its identifier. If there are multiple versions of sessions for the same session identifier, latest version of the session is returned by default. Use the optional query parameter ```sessionVersion``` to query a specific version.

Endpoint
```
GET api/v1/connections/{connection name}/sessions/identifier/{identifier}
```

Example  
```
GET api/v1/connections/Connection/sessions/identifier/TestSession1
```

Result  
```json
{
  "id": "test-session-id",
  "streamId": "24dc48bd-e0d9-4b29-9db9-e1d6301aaf7c",
  "identifier": "TestSession1",
  "timeOfRecording": "2018-12-03T00:00:00Z",
  "sessionType": "StreamingSession",
  "start": "2019-03-28T12:36:10.6907316Z",
  "end": "2019-03-28T12:50:22.6907316Z",
  "lapsCount": 0,
  "state": "Open",
  "topicName": "TopicName1",
  "group": "Group10",
  "version": 2,
  "configuration": "{ \"key\": \"value\" }",
  "sessionDetails": []
}
```

Optional parameters  
-------------------  

The ```/sessions/identifier``` resource supports the following optional parameters.
  
| Parameter name | Description                                                 | Default value | Example                                                                   |  
|----------------|-------------------------------------------------------------|---------------|---------------------------------------------------------------------------|
| sessionVersion | Session version.                                            | Highest available version | 3                                                             | 

<br />

### Query all sessions by their sessions ids

The API resource can be used to query multiple sessions using the session ids. ```sessionIds``` route parameter is a comma-separated list of session ids. When more than one session exists for a session id, the latest version of the session is returned by default.

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionIds}
```

Example  
```
GET api/v1/connections/Connection/sessions/01388874-7a80-4d86-8020-8709c278fc9a,0151d834-7a23-46c6-a3fc-eb536adcf93b
```

Result  
```json
[
  {
    "id": "01388874-7a80-4d86-8020-8709c278fc9a",
    "streamId": "24dc48bd-e0d9-4b29-9db9-e1d6301aaf7c",
    "identifier": "Identifier3",
    "timeOfRecording": "2018-11-24T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T15:24:23.5599596Z",
    "end": "2019-03-28T15:24:23.5599596Z",
    "lapsCount": 0,
    "state": "Closed",
    "topicName": "TopicName3",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  },
  {
    "id": "0151d834-7a23-46c6-a3fc-eb536adcf93b",
    "streamId": "7aaeb737-217e-4e27-ac6d-55a22bd530f2",
    "identifier": "Identifier7",
    "timeOfRecording": "2018-12-09T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-28T14:33:08.3917995Z",
    "end": "2019-03-28T14:33:08.3917995Z",
    "lapsCount": 0,
    "state": "Open",
    "topicName": "TopicName7",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  }
]
```

Optional parameters  
-------------------  

The ```/sessions/{sessionIds}``` resource supports the following optional parameters.
  
| Parameter name | Description                                                 | Default value | Example                                                                   |  
|----------------|-------------------------------------------------------------|---------------|---------------------------------------------------------------------------|
| sessionVersion | Session version.                                            | Highest available version | 3                                                             |

Live sessions  
===================  

The ```/sessions/live``` endpoint gives you access to a list of **live** sessions available for a given connection. You can filter this list of sessions by several optional parameters.

Optional parameters  
-------------------  
  
| Parameter name | Description                                                 | Default value | Example       |  
|----------------|-------------------------------------------------------------|---------------|---------------|  
| filter         | It allows filtering on the results.                         |               | lapsCount > 5 |  
| page           | Index of page returned in result (0 is first page)          |               | 3             |  
| pageSize       | Size of one page.                                           | 50            | 100           |  

Endpoint
```
GET api/v1/connections/{connection name}/sessions/live
```
  
Example  
```
GET api/v1/connections/Simulator/sessions/live
```

Result
```json
[
  {
    "id": "06f2d6d8-5811-48f0-a7a0-50e84db12704",
    "streamId": "7aaeb737-217e-4e27-ac6d-55a22bd530f2",
    "identifier": "Identifier10",
    "timeOfRecording": "2018-11-29T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2019-03-29T08:34:01.9697625Z",
    "end": "2019-03-29T08:34:01.9697625Z",
    "lapsCount": 0,
    "state": "Open",
    "topicName": "TopicName10",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails": []
  }
]
```

Parameters  
==========  

The ```/sessions/{sessionId}/parameters``` endpoint gives you access to a list of **parameters** available for a specific session. The list of **parameters** of a session are the fields related to the data that we can consume as described in [Consuming Data] (/docs/ConsumingData.md) section. If more than one session exists for the same session id, parameters of the latest version of the session are returned by default.

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters
```

Example  
```
GET api/v1/connections/Connection/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/parameters
```
  
Optional parameters  
-------------------  
  
| Parameter name | Description                                                 | Example           |  
|----------------|-------------------------------------------------------------|-------------------|  
| page           | Index of page returned in result (0 is first page)          | 3                 |  
| pageSize       | Size of one page.                                           | 50                |  
| contains       | Text filter applied to the <ins>identifier</ins> parameter. | vCar              |
| startsWith     | Text filter applied to the <ins>identifier</ins> parameter  | vCar              |
| filter         | It allows filtering on the results.                         | Frequency;ge;10   |
| order          | It allows ordering of the results.                          | MaximumValue:desc |
| sessionVersion | Session version.                                            | 3                 |

### Paging  

Example  
```
GET api/v1/connections/M800960/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/parameters?page=2&pageSize=50
```
  
### Filtering

It is possible to provide filtering for parameters.
  
Example 
```
GET api/v1/connections/Connection/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/parameters?contains=vCar
```
  
Result  
```json
[
  {
    "identifier": "vCar:APP1",
    "name": "vCar",
    "maximumValue": 100,
    "minimumValue": 0,
    "description": "fTAGDisp_vCar",
    "units": "",
    "format": "%5.2f",
    "parentParameterGroup": "APP1",
    "frequency": 0,
    "aggregates": "Avg",
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "identifier": "vCar:APP2",
    "name": "vCar",
    "maximumValue": 360,
    "minimumValue": 0,
    "description": "Car speed",
    "units": "",
    "format": "%5.1f",
    "parentParameterGroup": "APP2",
    "frequency": 0,
    "aggregates": "Avg",
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "identifier": "vCar_2Buffer:APP8",
    "name": "vCar_2Buffer",
    "maximumValue": 400,
    "minimumValue": 0,
    "description": "fTAGDisp_vCar_2Buffer",
    "units": "",
    "format": "%5.2f",
    "parentParameterGroup": "APP8",
    "frequency": 0,
    "aggregates": "Avg",
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "identifier": "vCarDemo:APP1",
    "name": "vCarDemo",
    "maximumValue": 400,
    "minimumValue": 0,
    "description": "fTAGDisp_vCarDemo",
    "units": "",
    "format": "%5.2f",
    "parentParameterGroup": "APP1",
    "frequency": 0,
    "aggregates": "Avg",
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "identifier": "vCarEndofMarpleSector1:APP3",
    "name": "vCarEndofMarpleSector1",
    "maximumValue": 20,
    "minimumValue": 0,
    "description": "fTAGDisp_vCarEndofMarpleSector1",
    "units": "",
    "format": "%5.2f",
    "parentParameterGroup": "APP3",
    "frequency": 0,
    "aggregates": "Avg",
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  }
]
```

```streamId``` is a unique identifier that identifies a specific session version.

Details  
=======

The ```/sessions/{sessionId}/details``` endpoint gives you access to a list of **details** available for a specific session. When more than one session is available for a session id, latest version of the session is returned. Use a filter on the ```version``` field to access details of a specific session version.

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/details
```
  
Example  
```
GET api/v1/connections/Connection/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/details
```
  
Result  
```json
[
  {
    "id": "92ce7a51-83d1-43ec-bb0a-9cda685ca47c",
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704",
    "identifier": "Identifier6",
    "timeOfRecording": "2018-11-30T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2017-03-29T08:34:01.9697625Z",
    "end": "2017-03-29T08:34:01.9697625Z",
    "lapsCount": 0,
    "state": "Closed",
    "topicName": "TopicName6",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value\" }",
    "sessionDetails":
    [
      {
          "Name": "Session Description",
          "Value": "P1GTR"
      },
      {
          "Name": "Session Name",
          "Value": "GAS R1"
      },
      {
          "Name": "Session Number",
          "Value": "R1"
      },
      {
          "Name": "Driver",
          "Value": "GAS"
      },
      {
          "Name": "Car",
          "Value": "P1GTR"
      },
      {
          "Name": "Circuit",
          "Value": "Bar"
      },
      {
          "Name": "Race/Test",
          "Value": "Simulator"
      },
      {
          "Name": "Pit Lane Trigger",
          "Value": "None"
      },
      {
          "Name": "Unit Data Source",
          "Value": "vTAG RF,Ethernet Telemetry/Wirelink "
      },
      {
          "Name": "Date of recording",
          "Value": "07/08/2017"
      }
    ]
  }
]
```

Laps  
====  

The ```/sessions/{sessionId}/laps``` endpoint gives you access to information related to the **laps** of a specific session.
This endpoint provides a set of optional parameters.

| Parameter name | Description                                                 | Example           |  
|----------------|-------------------------------------------------------------|-------------------|  
| filter         | It allows filtering on the results.                         | number;ge;10      |
| order          | It allows ordering of the results.                          | number:desc       |
| sessionVersion | Session version.                                            | 3                 |

### Query all laps from a given session

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/laps
```
  
Example 
```
GET api/v1/connections/Connection/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/laps
```
  
Result
```json
[
  {
    "start": "1970-01-01T00:00:00Z",
    "end": "1970-01-01T00:00:00Z",
    "lapTime": 2,
    "countForFastestLap": true,
    "name": "Name2",
    "number": 2,
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "start": "1970-01-01T00:00:00Z",
    "end": "1970-01-01T00:00:00Z",
    "lapTime": 3,
    "countForFastestLap": false,
    "name": "Name3",
    "number": 3,
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "start": "1970-01-01T00:00:00Z",
    "end": "1970-01-01T00:00:00Z",
    "lapTime": 4,
    "countForFastestLap": true,
    "name": "Name4",
    "number": 4,
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "start": "1970-01-01T00:00:00Z",
    "end": "1970-01-01T00:00:00Z",
    "lapTime": 5,
    "countForFastestLap": false,
    "name": "Name5",
    "number": 5,
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  },
  {
    "start": "1970-01-01T00:00:00Z",
    "end": "1970-01-01T00:00:00Z",
    "lapTime": 6,
    "countForFastestLap": true,
    "name": "Name6",
    "number": 6,
    "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
  }
]
```

```streamId``` is a unique identifier that identifies a specific session version.

### Query a lap for a given session

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/laps
```
  
Example 
```
GET api/v1/connections/M800960/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/laps/5
```
  
Result
```json
{
  "start": "1970-01-01T00:00:00Z",
  "end": "1970-01-01T00:00:30Z",
  "lapTime": 30000000000,
  "countForFastestLap": false,
  "name": "Lap",
  "number": 5,
  "streamId": "06f2d6d8-5811-48f0-a7a0-50e84db12704"
}
```
