# ![logo](/Branding/branding.png) SignalR Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Service configuration**](ServiceConfig.md)<br>
- **API**<br>
  - [Subscribe to parameter by stream](#subscribe-to-parameter-by-stream)
  - [Unsubscribe from parameter by stream](#unsubscribe-from-parameter-by-stream)
  - [Unsubscribe from stream](#unsubscribe-from-stream)
  - [Subscribe to session update](#subscribe-to-session-update)
  - [Unsubscribe from session update](#unsubscribe-from-session-update)
  - [Subscribe to live sessions](#subscribe-to-live-sessions)
  - [Unsubscribe from live sessions](#unsubscribe-from-live-sessions)

The service is built using SignalR, therefore we suggest the use of the an [official Microsoft library](https://docs.microsoft.com/en-us/aspnet/core/signalr/javascript-client) for SignalR. The rest of the documentation will assume you're using one of these libraries.

# Authentication
The service is authenticating against the OAuth provider defined in [service config](ServiceConfig.md). Examples of how to pass token to SignalR service can be found [here](https://docs.microsoft.com/en-us/aspnet/core/signalr/authn-and-authz).

# Telemetry Data Hub
The telemetry data bub is the only hub available on the API. You can subscribe to various endpoints to be notified about new or closing streams. Also, you can subscribe to new parameter data notifications.

TypeScript example
``` TypeScript
var hubConnection = new HubConnectionBuilder()
    .withUrl('http://your-signalr-service-endpoint/TelemetryDataHub', { accessTokenFactory: getToken })
    .build();
```

## Subscribe to parameter by stream
Subscribes to a parameter's new data notifications for a stream.

#### Telemetry Data model

| Property | Description |
| - | - |
| topicName | Name of the topic the session is streaming on|
| streamId | The unique id of the stream this TelemetryData belongs to |
| epoch | The offset used for all timestamps since 01/01/1970 in nanoseconds |
| parameters | A dictionary of parameter identifier to ParameterData |

#### Parameter Data Model

| Property | Description |
| - | - |
| timestamps | Array of timestamps in nanoseconds since epoch. Timestamps are linked to values by index |
| values | Array of values. Values are linked to timestamps by index |

``` TypeScript
// TypeScript example
// subscribe to as many parameters as you wish using line below
hubConnection.invoke('SubscribeToParameterByStream', parameterIdentifier, streamId);

// then the parameter data will be delivered to clients using callback
hubConnection.on('OnTelemetryDataReceived', (data : TelemetryData) =>
    {
        // do something with `data`
    });
```

## Unsubscribe from parameter by stream

Unsubscribes from a single parameter's data notifications for a stream the [hub connection previously subscribed to](#subscribe-to-parameter-by-stream).

``` TypeScript
// TypeScript example
hubConnection.invoke('UnsubscribeFromParameterByStream', parameterIdentifier, streamId);
```


## Unsubscribe from stream

Unsubscribes from all parameter data notifications for a stream the [hub connection previously subscribed to](#subscribe-to-parameter-by-stream).

``` TypeScript
// TypeScript example
hubConnection.invoke('UnsubscribeFromStream', streamId);
```

## Subscribe to session update
Notifies about session metadata or state change.

#### Session Model
| Property | Description |
|-|-|
| topicName | Name of the topic the session is streaming on |
| streamId | The unique id of this stream |
| sessionName | The name of the session, also known as Identifier in TAP |
sessionDetails | Detail name (string) to detail value (string) dictionary |
| state | State of the session as string. Possible values are `Unknown`, `Waiting`, `Open`, `Closed`, `Truncated`, `Failed`, `Abandoned` |
| sessionStart | DateTime using ISO 8601 |
| dataState | Parameter data state as a string. Possible values:<br>`NoData`: Session is live but no data yet <br>`Interrupted`: Session is live and had data, but not in last 10 seconds<br>`Running`: Session is live and is streaming data <br> `Closed`: Session is closed


``` TypeScript
// TypeScript example
// subscribe to the session updates and get the initial state delivered via callback
hubConnection.invoke('SubscribeToSessionUpdate')
    .then((sessions: { [index: string]: Session } ) => {
        // `sessions` is a dictionary of streamId to StreamSession containing initial session states
    }

// then the session update will be delivered to the client using callback
hubConnection.on('OnSessionUpdate', async (session: Session) => {
        // `session` is the updated session model
    });    
```

## Unsubscribe from session update
Unsubscribes from session updates the [hub connection previously subscribed to](#subscribe-to-session-update).

``` TypeScript
// TypeScript example
hubConnection.invoke('UnsubscribeFromSessionUpdate');
```

## Subscribe to live sessions
Notifies about current live sessions whenever one changes. The benefit of subscribing to live sessions endpoint over session update is that the client doesn't have to maintain the list of current live sessions. However, this uses more bandwidth, as even if only one live session changes the entire set is transmitted.

#### Session Model
| Property | Description |
|-|-|
| topicName | Name of the topic the session is streaming on |
| streamId | The unique id of this stream |
| sessionName | The name of the session, also known as Identifier in TAP |
| sessionDetails | String to string dictionary, where key is the name of the detail |
| state | State of the session. Possible values are `Unknown`, `Waiting`, `Open`, `Closed`, `Truncated`, `Failed`, `Abandoned` |
| sessionStart | DateTime using ISO 8601 |
| dataState | Parameter data state as a string. Possible values:<br>`NoData`: Session is live but no data yet <br>`Interrupted`: Session is live and had data, but not in last 10 seconds<br>`Running`: Session is live and is streaming data <br> `Closed`: Session is closed

``` TypeScript
// TypeScript example
// subscribe to the live session updates and get the current live sessions delivered via callback
hubConnection.invoke('SubscribeToLiveSessionUpdate')
    .then((sessions: { [index: string]: Session } ) => {
        // `sessions` is a dictionary of streamId to SessionModel containing session states
    }

// then whenever a live session changes the entire set will be delivered via callback
hubConnection.on('OnLiveSessionUpdate', async (sessions: { [index: string]: Session } ) => {
        // `sessions` is a dictionary of streamId to SessionModel containing session states
    });    
```
## Unsubscribe from live sessions

Unsubscribes from live session updates the [hub connection previously subscribed to](#subscribe-to-live-sessions).

``` TypeScript
// TypeScript example
hubConnection.invoke('UnsubscribeFromLiveSessions');
```