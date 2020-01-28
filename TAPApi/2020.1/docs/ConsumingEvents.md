# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Getting started**](GettingStarted.md)<br>
- [**Authorization**](Authorization.md)<br>
- [**Querying Metadata**](Metadata.md)<br>
- [**Consuming Data**](ConsumingData.md)<br>
- **Consuming Events**<br>
- [**Session Versions**](SessionVersions.md)<br>

Consuming Events
=====================

This section describes what endpoints are available and how to consume **Events data**.<br />

### Base url masks

It is possible to consume events data using `/events/{eventId}/data` endpoint.

```
GET api/v1/connections/{connection name}/sessions/{sessionId}/events/{event_1,event_2,...,event_n}/data
```

### Url parameters

| Parameter name | Description                                                                                         |    Example                                |
|----------------|-----------------------------------------------------------------------------------------------------|-------------------------------------------|
| connection     | Connection name.                                                                                    |    `RawData`                            |
| sessionId      | Session Id.                                                                                        |    `016fa61e-33e2-7e85-1bc9-4ab56c668136` |
| event          | Name(s) of the events(s). *Multiple events can be requested by separating them with a comma* |    `2938:TAG320BIOS,86D1:Chassis`           |

Example
```
GET api/v1/connections/Connection/sessions/b247f901-1a7c-4e78-847c-66843d62941d/events/2938:TAG320BIOS,86D1:Chassis/data 
```

### Optional parameters

Optionally it is possible to filter **Events data** by the following parameters:

| Parameter name | Description                                                                                                                         |    Default value    |    Example                               |
|----------------|-------------------------------------------------------------------------------------------------------------------------------------|---------------------|------------------------------------------|
| from           | Filters data in session by time in microseconds or nanoseconds. All data returned would have a time stamp after the specified time. |    `null`           |    1546347600/1546347600000              |
| to             | Filters data in session by time in microseconds or nanoseconds. All data returned would have a time stamp before the specified time.|    `null`           |    1546348200/1546348200000              |
| lap            | Filters data for specified lap(s) in session.                                                                                          |    `null`           |    `lap=3&lap=4&lap=5`                                     |
| dataFilter     | Data filter with comparison operators.                                                                                              |    `null`           |    `86D1:Chassis;ge;300`                 |
| tagFilter      | Tag filter with comparison operators.                                                                                               |    `null`           |    `lapnumber;eq;2`                      |
| page           | Index of page returned in result (0 is first page)                                                                                  |      0              |    3                                     |
| pageSize       | Size of one page.                                                                                                                   |    `null`           |    50                                    |
| timeUnit       | Units of time. Use `Ns` for nanoseconds and `Ms` for microseconds.                                                                  |      `Ns`           |    `Ms`                                  |
| groupBy        | Groups queries by tags. Use `*` to group by all available tags.                           |    `null`           |    `groupBy=lapnumber` |


### More on filtering

The API currently supports filtering events data (dataFilter), i.e. the actual values for events, and filtering tags (tagFilter), i.e. the metadata associated with the values.

#### Events data filter types (dataFilter)

| Shortcut | Full name             |
|----------|-----------------------|
| eq       | Equal                 |
| gt       | Greater than          |
| ge       | Greater than or equal |
| lt       | Less than             |
| le       | Less than or equal    |
| ne       | Not equal             |

#### Events data filter construction (dataFilter)

Structure of one event data filter is:

Events data filter url mask
```
{EventId};{filterOperationShortcut};{value}
```

Events data filter example
```
86D1:Chassis;gt;5
```

Example of filtering values that have 86D1:Chassis between 5 and 20: 

```
86D1:Chassis;gt;5,86D1:Chassis;le;20
```

Note that some Event Ids have a colon ( : ) in, so we use semicolons ( ; ) for the filtering.

#### Tag filter types (tagFilter)

| Shortcut | Full name             |
|----------|-----------------------|
| eq       | Equal                 |
| ne       | Not equal             |

#### Tag filter construction (tagFilter)

Structure of one tag filter is:

Tag filter url mask
```
{tag};{filterOperationShortcut};{value}
```

Events data filter example
```
lapnumber;eq;3
```

Events Data example
====================

Find here below an example of a query of the events `2938:TAG320BIOS` and `86D1:Chassis` grouped by `lapnumber`.

Example
```
GET api/v1/connections/Connection/sessions/b247f901-1a7c-4e78-847c-66843d62941d/events/2938:TAG320BIOS,86D1:Chassis/data?timeUnit=Ms&groupBy=lapnumber 
```

CSV Result:
```
time,tagValues,status,eventId,value1,value2,value3
1579861123608000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861164705000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861228445000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861233491000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861261466000,lapnumber=1,,86D1:Chassis,11,1,4
1579861267126000,lapnumber=1,,86D1:Chassis,11,14,1
1579861267146000,lapnumber=1,,86D1:Chassis,5,4,1
1579861279813000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861289206000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861296315000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861303880000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861304666000,lapnumber=1,,86D1:Chassis,5,1,4
1579861313069000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861359775000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861363817000,lapnumber=1,,2938:TAG320BIOS,3,0,0
1579861363817000,lapnumber=2,,2938:TAG320BIOS,3,0,0
1579861370325000,lapnumber=2,,2938:TAG320BIOS,3,0,0
```

JSON result:
```Json
{
    "eventData": {
        "2938:TAG320BIOS": {
            "timeStamps": [
                1579861123608000,
                1579861164705000,
                1579861228445000,
                1579861233491000,
                1579861279813000,
                1579861289206000,
                1579861296315000,
                1579861303880000,
                1579861313069000,
                1579861359775000,
                1579861363817000,
                1579861363817000,
                1579861370325000
            ],
            "lapNumbers": [
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0,
                0
            ],
            "status": [
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                "",
                ""
            ],
            "values": [
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ],
                [
                    3.0,
                    0.0,
                    0.0
                ]
            ],
            "tagValues": [
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "2"
                },
                {
                    "lapnumber": "2"
                }
            ]
        },
        "86D1:Chassis": {
            "timeStamps": [
                1579861261466000,
                1579861267126000,
                1579861267146000,
                1579861304666000
            ],
            "lapNumbers": [
                0,
                0,
                0,
                0
            ],
            "status": [
                "",
                "",
                "",
                ""
            ],
            "values": [
                [
                    11.0,
                    1.0,
                    4.0
                ],
                [
                    11.0,
                    14.0,
                    1.0
                ],
                [
                    5.0,
                    4.0,
                    1.0
                ],
                [
                    5.0,
                    1.0,
                    4.0
                ]
            ],
            "tagValues": [
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                },
                {
                    "lapnumber": "1"
                }
            ]
        }
    }
}
```
