# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Getting started**](GettingStarted.md)<br>
- [**Authorization**](Authorization.md)<br>
- [**Querying Metadata**](Metadata.md)<br>
- [**Consuming Data**](ConsumingData.md)<br>
- **Session Versions**<br>


## Session Versions

In TAP, a session can be identified using various identifiers some of which are guarenteed to be unique everytime you stream a session using TAP while others aren't. Following are the identifiers you can use to identify a session.

1. Stream Id <br>
Stream Id is a GUID that is generated each time you stream a session from TAP. This is guarenteed to be unique. For example, if you are replaying the same historic session from a file multiple times, you will get a new `streamId` for each replay.
2. Id <br>
Session Id, which is also a GUID, is unique per session up to a replay. In other words, if you replay the same historic session, session id does not change between replays.
3. Identifier <br>
Identifer is simply a human-friendly name of the session. This is unique up to a replay just like the Id.

Even though the `streamId` is guarenteed to be unique, its usefulness is rather limited as this is an auto-generated GUID from within TAP which doesn't say much about the session (for example, information about the origin of the session). `id` is the most meaningful identifier in terms of keeping track of a session across Atlas ecosystem as well as TAP like tracing a TAP session to an ADS (Atlas Data Server) session. However, the fact that `id` is not unique between replays of the same session in TAP calls for a versioning system for sessions within TAP.

#### Why replay a session?

The same historic session may be replayed in TAP for various reasons. Some of the more common uses for replaying a session are:

1. Play a down-sampled version of the original telemetry session;
2. Run data science models on the same telemetry session in which the models themselves may be versioned.

In order to support scenarios like these, we have introduced session versioning in TAP which allows you to group and version sessions along with metadata about any custom analytics models you may have used during a session replay.

The json representation of a session model:

```
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
    "sessionDetails": [],
    "label": "*"
  }
```

In the above json, `group`, `version` and `configuration` carry information about a session version. `group` indicates any group this session model belongs to. `version` indicates the session version which you are free to use the way best fit your needs (e.g. version the session together with the data science model, not increment the version at all, etc.). You can use `configuration` property to store detailed information about the session version (e.g. a stringified representation of the data science model such as a json).

#### Querying session versions

Detailed information on querying sessions are described in [Querying Metadata](Metadata.md). Here we will focus on querying session versions. In TAP API, as far as the client interface concerned, sessions are identifed using the session `id` (not the stream id) to make sure that you can identify a session uniformly across the Atlas ecosystem (Atlas desktop client, ADS, etc) which uses session `id` as the primary session identifier. If there are multiple session versions with the same session `id`, you must specify the version of a session in TAP API using the optional `sessionVersion` query parameter.

If you do not specify the session version, TAP API returns the latest version of the session in storage. The latest version is determined, in order of precedence, using the `version`, quality (an internal value related to Atlas sessions) and time of recording. If you did not version the sessions or the session quality is not applicable to your usecase, TAP API will return the last recorded session based on the `timeOfRecording` for that session id.

You can use ```/sessions/{sessionId}/versions``` resource to explore different versions of the session.<br />

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{session id}/versions
```

Example  
```
GET api/v1/connections/Connection/sessions/0151d834-7a23-46c6-a3fc-eb536adcf93b/versions
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
    "state": "Closed",
    "topicName": "TopicName",
    "group": "Group10",
    "version": 1,
    "configuration": "{ \"key\": \"value1\" }",
    "sessionDetails": [],
    "label": "*"
  },
  {
    "id": "0151d834-7a23-46c6-a3fc-eb536adcf93b",
    "streamId": "08fb78fa-6547-4a60-8a58-f3d6c1dd2978",
    "identifier": "Identifier5",
    "timeOfRecording": "2018-11-29T00:00:00Z",
    "sessionType": "StreamingSession",
    "start": "2018-03-28T14:33:08.3917995Z",
    "end": "2018-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName",
    "group": "Group10",
    "version": 2,
    "configuration": "{ \"key\": \"value2\" }",
    "sessionDetails": [],
    "label": "*"
  },
  {
    "id": "0151d834-7a23-46c6-a3fc-eb536adcf93b",
    "streamId": "65fd2589-9359-49d6-bc1c-5a69128358e3",
    "identifier": "Identifier4",
    "timeOfRecording": "2018-11-23T00:00:00Z",
    "sessionType": "StreamingSession",
   "start": "2018-03-28T14:33:08.3917995Z",
    "end": "2018-03-28T14:33:08.3917995Z",
    "lapsCount": 10,
    "state": "Closed",
    "topicName": "TopicName",
    "group": "Group10",
    "version": 3,
    "configuration": "{ \"key\": \"value3\" }",
    "sessionDetails": [],
    "label": "*"
  }
]
```





