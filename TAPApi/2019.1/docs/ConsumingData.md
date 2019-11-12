# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Getting started**](GettingStarted.md)<br>
- [**Authorization**](Authorization.md)<br>
- [**Querying Metadata**](Metadata.md)<br>
- **Consuming Data**<br>
- [**Session Versions**](SessionVersions.md)<br>

Consuming Data
=====================

This section describes what endpoints are available and how to consume parameter data.<br />
URIs for consuming data are grouped under the namespace `/data`.<br />
It is possible to consume data using the api from either InfluxDB or SqlRace, being possible to retrieve the data at raw rate or at a given frequency in either case.

- When using <ins>InfluxDb</ins> as the backing data storage, a wide variety of aggregations are supported, for example: Count, Mean, Median, Sum, First, Last, Max, Min, Stddev. *NOTE: Distinct is currently not supported for multiple parameters*
- When using <ins>SqlRace</ins> as the backing data storage, First, Mean, Min, Max aggregations are supported.
- The resource `/data/aggregate` is only supported by InfluxDB backing data storage.
- All resources under `/data` for both data storages provide different response formats. All endpoints support both json and csv, by specifying header Accept-Type as "application/json" or "text/csv", the later one being more optimized for downloading large amounts of parameter data. This flexible approach allows for a data format to be chosen given the technical requirements of the client application (e.g. web applications). Both data storages allow you to query one or multiple parameters for a given session.
- When using <ins>InfluxDb</ins> as the backing data storage, it is possible to access session versions using optional query parameters. If a version is not specified, latest version for a specific session is used to query by default.

## Consuming Parameter Data

It is possible to consume parameter data using `/data` endpoint. For resources under `/data` path, two url masks are available for querying data.

### Base url masks

Querying at raw rate:

```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1,parameter_2,...,parameter_n}/data
```

### Url parameters

| Parameter name | Description                                                                                         |    Example                                |
|----------------|-----------------------------------------------------------------------------------------------------|-------------------------------------------|
| connection     | Connection name.                                                                                    |    `SQLRACE01`                            |
| sessionId      | Session Id.                                                                                        |    `016fa61e-33e2-7e85-1bc9-4ab56c668136` |
| parameter      | Name(s) of the parameter(s). *Multiple parameters can be requested by separating them with a comma* |    `vCar:Chassis, gLat:Chassis`           |

<br />

Query with frequency:

```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1;aggregation_1,...,parameter_n;aggregation_n};/{frequency}/data 
```

### Url parameters

| Parameter name | Description                                                                                                                  |    Default value    |    Example                                |
|----------------|------------------------------------------------------------------------------------------------------------------------------|---------------------|-------------------------------------------|
| connection     | Connection name.                                                                                                             |                     |    `SQLRACE01`                            |
| sessionId      | Session Id.                                                                                                                 |                     |    `016fa61e-33e2-7e85-1bc9-4ab56c668136` |
| parameter      | Name(s) of the parameter(s). *Multiple parameters can be requested by separating them with a comma*                          |                     |    `vCar:Chassis, gLat:Chassis`           |
| aggregation    | Optional aggregation function separated by a semicolon `;`. *Do not add semicolon if you are not specifying an aggregation.* |    `mean`           |    `;max`                                 |
| frequency      | Frequency of the results in Hz.                                                                                              |                     |    10                                     |

<br />

### Optional parameters for both base urls

Optionally it is possible to filter the data by the following parameters:

| Parameter name | Description                                                                                                                         |    Default value    |    Example                               |
|----------------|-------------------------------------------------------------------------------------------------------------------------------------|---------------------|------------------------------------------|
| from           | Filters data in session by time in microseconds or nanoseconds. All data returned would have a time stamp after the specified time. |    `null`           |    1546347600/1546347600000              |
| to             | Filters data in session by time in microseconds or nanoseconds. All data returned would have a time stamp before the specified time.|    `null`           |    1546348200/1546348200000              |
| lap            | Filters data for specified lap in session.                                                                                          |    `null`           |    3                                     |
| dataFilter     | Data filter with comparison operators.                                                                                              |    `null`           |    `vCar:Chassis;ge;300`                 |
| tagFilter      | Tag filter with comparison operators.                                                                                               |    `null`           |    `lapnumber;eq;2`                      |
| page           | Index of page returned in result (0 is first page)                                                                                  |      0              |    3                                     |
| pageSize       | Size of one page.                                                                                                                   |    `null`           |    50                                    |
| timeUnit       | Units of time. Use `Ns` for nanoseconds and `Ms` for microseconds.                                                                  |      `Ns`           |    `Ms`                                  |
| groupBy        | Groups queries by fields (Only supported for InfluxDb storage). Use `*` to group by all available fields.                           |    `null`           |    `groupBy=lapnumber&groupBy=sessionId` |
| sessionVersion | Version of a session to use. If not specified, latest version is used.                                                              |    `null`           |    2    |

#### Optional parameters validation
As mentioned when requesting either raw data or frequency one, it is possible to provide a set of optional parameters. The API will validate the optional parameters provided and in case of failure in binding any of the parameters it will provide a response with a 422 status code (Unprocessable Entity) and a message or a set of messages with the failure errors. <br />
This validation layer is provided only for API using InfluxDb as data storage. The parameters filter and groupby are excluded from model validation.

##### Query with one wrong parameter

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,gLat:Chassis;mean/data?Lap=test
```

JSON result:
```Json
{
  "message": "Model state validation failed",
  "errors": [
    {
      "field": "Lap",
      "message": "The value 'test' is not valid for Lap."
    }
  ]
}
```

##### Query with multiple wrong parameter 

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,gLat:Chassis;mean/data?page=-1&pageSize=abc
```

JSON result:
```Json
{
  "message": "Model state validation failed",
  "errors": [
    {
      "field": "Page",
      "message": "Please enter a positive number"
    },
    {
      "field": "PageSize",
      "message": "The value 'abc' is not valid for PageSize."
    }
  ]
}
```

### More on filtering

The API currently supports filtering parameter data, i.e. the actual values for parameters, and filtering tags, i.e. the metadata associated with the values.
The parameter data filter value is compared to the aggregated values obtained from the database, e.g. Mean.

#### Parameter data filter types

| Shortcut | Full name             |
|----------|-----------------------|
| eq       | Equal                 |
| gt       | Greater than          |
| ge       | Greater than or equal |
| lt       | Less than             |
| le       | Less than or equal    |
| ne       | Not equal             |

#### Parameter data filter construction

Structure of one parameter data filter is:

Parameter data filter url mask
```
{parameterName};{filterOperationShortcut};{value}
```

Parameter data filter example
```
vCar:Chassis;gt;300
```

Example of filtering values that have vCar between 100 and 150 kph. 

Parameter data filter setting example
```
vCar:Chassis;gt;100,vCar:Chassis;le;150
```

Note that some parameters have a colon ( : ) in, so we use semicolons ( ; ) for the filtering.

#### Tag filter types

| Shortcut | Full name             |
|----------|-----------------------|
| eq       | Equal                 |
| ne       | Not equal             |

#### Tag filter construction

Structure of one tag filter is:

Tag filter url mask
```
{tag};{filterOperationShortcut};'{value}'
```

Parameter data filter example
```
lapnumber;eq;'3'
```

Query Parameter Data
====================

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1,parameter_2,...,parameter_n}/data
```

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,gLat:Chassis;mean/data?pageSize=10
```

CSV Result:
```
time,tagValues,vCar:Chassis,gLat:Chassis
1549534712083000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.2,
1549534712093000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.2,
1549534712103000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.15,
1549534712113000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.11,
1549534712123000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.11,
1549534712133000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.15,
1549534712143000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.21,
1549534712153000000,sessionId=64192714-90fe-4865-a191-a6887e465184,1.26,
1549534712163000000,sessionId=64192714-90fe-4865-a191-a6887e465184,0.46,
1549534712167000000,sessionId=64192714-90fe-4865-a191-a6887e465184,,0.025
```

JSON result:
```Json
{
  "parameters": {
    "vCar:Chassis": {
      "timestamps": [
        1549534712083000000,
        1549534712093000000,
        1549534712103000000,
        1549534712113000000,
        1549534712123000000,
        1549534712133000000,
        1549534712143000000,
        1549534712153000000,
        1549534712163000000
      ],
      "values": [
        1.2,
        1.2,
        1.15,
        1.11,
        1.11,
        1.15,
        1.21,
        1.26,
        0.46
      ]
    },
    "gLat:Chassis": {
      "timestamps": [
        1549534712167000000
      ],
      "values": [
        0.025
      ]
    }
  }
}
```

Query Parameter Data Frequency
==============================

## Query Parameter Data Frequency with a timestamp range

It is demonstrated in this section several requests and combinations of the filtering capabilities of the API. All combinations are possible and these are also available to be requested without frequency as well.

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1;aggregation_1,parameter_2;aggregation_2,...,parameter_n;aggregation_n}/{frequency}/data
```

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,vCar:Chassis;max,gLat:Chassis;mean/10/data?pageSize=10&from=1549534714876000000&to=1549572500653000000
```

CSV Result:
```
time,tagValues,Mean(vCar:Chassis),Max(vCar:Chassis),Mean(gLat:Chassis)
1549534714800000000,,0,0,0.014
1549534714900000000,,0,0,-0.00525
1549534715000000000,,0,0,0.00475
1549534715100000000,,0,0,0.00625
1549534715200000000,,0,0,0.00475
1549534715300000000,,0,0,0.00075
1549534715400000000,,0,0,0.0045
1549534715500000000,,0,0,0.00575
1549534715600000000,,0,0,0.0085
1549534715700000000,,0,0,0.00575
```

JSON result:
```Json
{
  "timestamps": [
    1549534714800000000,
    1549534714900000000,
    1549534715000000000,
    1549534715100000000,
    1549534715200000000,
    1549534715300000000,
    1549534715400000000,
    1549534715500000000,
    1549534715600000000,
    1549534715700000000
  ],
  "values": {
    "Mean(vCar:Chassis)": [
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0
    ],
    "Max(vCar:Chassis)": [
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0,
      0.0
    ],
    "Mean(gLat:Chassis)": [
      0.013999999999999999,
      -0.0052499999999999995,
      0.004749999999999999,
      0.00625,
      0.004749999999999999,
      0.00075000000000000012,
      0.0045,
      0.0057500000000000008,
      0.0084999999999999989,
      0.005749999999999999
    ]
  }
}
```

## Query Parameter Data Frequency with a specific time unit (nanoseconds)

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1;aggregation_1,parameter_2;aggregation_2,...,parameter_n;aggregation_n}/{frequency}/data
```

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,gLat:Chassis;mean/10/data?pageSize=10&timeUnit=ns
```

CSV Result:
```
time,tagValues,Mean(vCar:Chassis),Mean(gLat:Chassis)
1549534712000000000,,1.2,
1549534712100000000,,0.762,0.00142857142857143
1549534712200000000,,1.214,0.002
1549534712300000000,,3.044,0.0055
1549534712400000000,,2.701,0.0065
1549534712500000000,,1.355,0.00375
1549534712600000000,,0.07,0.0005
1549534712700000000,,1.506,0.00075
1549534712800000000,,2.069,0.0015
1549534712900000000,,1.421,0.00175
```

JSON result
```Json
{
  "timestamps": [
    1549534712000000000,
    1549534712100000000,
    1549534712200000000,
    1549534712300000000,
    1549534712400000000,
    1549534712500000000,
    1549534712600000000,
    1549534712700000000,
    1549534712800000000,
    1549534712900000000
  ],
  "values": {
    "Mean(vCar:Chassis)": [
      1.2,
      0.7619999999999999,
      1.214,
      3.0440000000000005,
      2.7010000000000005,
      1.355,
      0.069999999999999993,
      1.5059999999999998,
      2.069,
      1.421
    ],
    "Mean(gLat:Chassis)": [
      "NULL",
      0.0014285714285714288,
      0.002,
      0.0055,
      0.0065000000000000006,
      0.00375,
      0.0004999999999999999,
      0.00075000000000000023,
      0.0014999999999999994,
      0.0017500000000000005
    ]
  }
}
```

## Query Parameter Data Frequency with a filter

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1;aggregation_1,parameter_2;aggregation_2,...,parameter_n;aggregation_n}/{frequency}/data
```

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,gLat:Chassis;mean/10/data?pageSize=10&dataFilter=vCar:Chassis;ge;300 
```

CSV Result:
```
time,tagValues,Mean(vCar:Chassis),Mean(gLat:Chassis)
1549534949900000000,,300.102,0.0414
1549534950000000000,,300.728,0.1048
1549534950100000000,,301.081,0.1378
1549534950200000000,,302.057,0.1194
1549534950300000000,,302.341,0.065
1549534950400000000,,302.818,0.0895
1549534950500000000,,303.211,-0.0263
1549534950600000000,,303.808,0.00690000000000001
1549534950700000000,,304.199,0.0133
1549534950800000000,,304.743,0.16
```

JSON result
```Json
{
  "timestamps": [
    1549534949900000000,
    1549534950000000000,
    1549534950100000000,
    1549534950200000000,
    1549534950300000000,
    1549534950400000000,
    1549534950500000000,
    1549534950600000000,
    1549534950700000000,
    1549534950800000000
  ],
  "values": {
    "Mean(vCar:Chassis)": [
      300.10200000000003,
      300.728,
      301.081,
      302.05700000000007,
      302.341,
      302.818,
      303.211,
      303.808,
      304.19900000000007,
      304.74300000000005
    ],
    "Mean(gLat:Chassis)": [
      0.041400000000000006,
      0.10480000000000002,
      0.13779999999999998,
      0.11940000000000003,
      0.065,
      0.0895,
      -0.026300000000000007,
      0.006900000000000006,
      0.013299999999999992,
      0.16000000000000003
    ]
  }
}
```

## Query Parameter Data Frequency with group by

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter_1;aggregation_1,parameter_2;aggregation_2,...,parameter_n;aggregation_n}/{frequency}/data
```

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis/10/data?pageSize=1&groupby=lapnumber
```

CSV Result:
```
time,tagValues,Mean(vCar:Chassis),Mean(gLat:Chassis)
1549534712000000000,lapnumber=1l,1.2,
1549535063400000000,lapnumber=2l,,-0.6745
1549535063200000000,lapnumber=3l,222.256,
1549535120200000000,lapnumber=4l,289.678571428571,0.764
1549535201500000000,lapnumber=5l,288.318888888889,1.18677777777778
1549535275900000000,lapnumber=6l,288.515,1.2145
1549535362100000000,lapnumber=7l,291.104,1.1925
```

JSON result
```Json
{
  "timestamps": [
    1549534712000000000,
    1549535063400000000,
    1549535063200000000,
    1549535120200000000,
    1549535201500000000,
    1549535275900000000,
    1549535362100000000
  ],
  "values": {
    "Mean(vCar:Chassis)": [
      1.2,
      "NULL",
      222.256,
      289.67857142857144,
      288.31888888888886,
      288.515,
      291.104
    ],
    "Mean(gLat:Chassis)": [
      "NULL",
      -0.6745,
      "NULL",
      0.764,
      1.1867777777777777,
      1.2144999999999997,
      1.1924999999999997
    ]
  },
  "tags": [
    {
      "lapnumber": "1l"
    },
    {
      "lapnumber": "2l"
    },
    {
      "lapnumber": "3l"
    },
    {
      "lapnumber": "4l"
    },
    {
      "lapnumber": "5l"
    },
    {
      "lapnumber": "6l"
    },
    {
      "lapnumber": "7l"
    }
  ]
}
```

#### Query Parameter Data Count

This functionality is available for `/data` with and without frequency.

Endpoint
```
GET api/v1/connections/{connection name}/sessions/{sessionId}/parameters/{parameter};{aggregation}/{frequency}/data/count
```

Example
```
GET api/v1/connections/Connection/sessions/92ce7a51-83d1-43ec-bb0a-9cda685ca47c/parameters/vCar:Chassis;mean,gLat:Chassis;mean/10/data/count
```

JSON result
```Json
[
    {
        "name":"Mean(vCar:Chassis)",
        "count":6236.0
    },
    {
        "name":"Mean(gLat:Chassis)",
        "count":6291.0
    }
]
```

#### Aggregate Parameter Data over Tags.

If your backing storage is InfluxDb, you can aggregate parameter data over InfluxDb tags.

Endpoint
```
GET api/v1/connections/{connections}/sessions/{sessionId}/parameters/{parameter};{aggregation}/data/aggregate?groupBy={tag1}&groupBy={tag2}
```

Example
```
GET api/v1/connections/Connection/sessions/64192714-90fe-4865-a191-a6887e465184/parameters/vCar:Chassis;mean,gLat:Chassis;mean/data/aggregate?groupBy=lapnumber
```

CSV Result
```
time,tagValues,vCar:Chassis,gLat:Chassis
1549534711876000000,lapnumber=1l,112.335688215992,0.18400058029484376
1549534711876000000,lapnumber=2l,199.965857988166,-0.82674907292954236
1549534711876000000,lapnumber=3l,225.134068751761,0.087585425383542262
1549534711876000000,lapnumber=4l,191.609365859654,0.2036408483625701
1549534711876000000,lapnumber=5l,209.018854306413,0.26592772152238897
1549534711876000000,lapnumber=6l,179.251327235393,0.25245055569364194
1549534711876000000,lapnumber=7l,186.329971860711,0.52127658846863922
```

JSON result
```Json
{
  "timestamps": [
    1549534711876000000,
    1549534711876000000,
    1549534711876000000,
    1549534711876000000,
    1549534711876000000,
    1549534711876000000,
    1549534711876000000
  ],
  "values": {
    "Mean(vCar:Chassis)": [
      112.335688215992,
      199.96585798816574,
      225.13406875176085,
      191.60936585965354,
      209.01885430641281,
      179.25132723539306,
      186.32997186071083
    ],
    "Mean(gLat:Chassis)": [
      0.18400058029484376,
      -0.82674907292954236,
      0.087585425383542262,
      0.2036408483625701,
      0.26592772152238897,
      0.25245055569364194,
      0.52127658846863922
    ]
  },
  "tags": [
    {
      "lapnumber": "1l"
    },
    {
      "lapnumber": "2l"
    },
    {
      "lapnumber": "3l"
    },
    {
      "lapnumber": "4l"
    },
    {
      "lapnumber": "5l"
    },
    {
      "lapnumber": "6l"
    },
    {
      "lapnumber": "7l"
    }
  ]
}
```

Query Parameter Data Aggregates for Multiple Sessions
=====================================================

This functionality allows you to query aggregate data from multiple sessions by either explicitly specifying the session ids, or by specifying criteria that filters the sessions your are interested in using tags associated with the data, session metadata, and time ranges.
This is only supported using InfluxDB as data storage for now.  

#### Query results from multiple nodes 
If the data is being returned from different InfluxDB nodes the response will contain results split by node as you can see in the examples below.

Example CSV Result for multiple nodes
```
time,tagValues,Mean(vCar:Chassis),Mean(sLap:Chassis)
1563027942645000000,InfluxDbUri=http://influxdb1/;DatabaseName=Driver_A;MeasurementName=Measurement_A,128.790105799527,1852.37860522758
1563028393209000000,InfluxDbUri=http://influxdb2/;DatabaseName=Driver_B;MeasurementName=Measurement_B,111.280858322507,1621.34225978648
```

Example JSON result for multiple nodes
```Json
{
    "timestamps": [
        1563027942645000000,
        1563028393209000000
    ],
    "values": {
        "Mean(vCar:Chassis)": [
            128.79010579952748,
            111.28085832250734
        ],
        "Mean(sLap:Chassis)": [
            1852.37860522758,
            1621.3422597864769
        ]
    },
    "tags": [
        {
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Driver_A",
            "MeasurementName": "Measurement_A"
        },
        {
            "InfluxDbUri": "http://influxdb2/",
            "DatabaseName": "Driver_B",
            "MeasurementName": "Measurement_B"
        }
    ]
}
```

#### Querying using multiple session ids

Endpoint
```
GET api/v1/connections/{connection name}/sessions/multi/{sessionId_1,sessionId_2,...,sessionId_n}/parameters/{parameter_1,parameter_2,...,parameter_n}/data/aggregate
```

Example
```
GET api/v1/connections/Connection/sessions/multi/4475966e-8096-46d5-b033-3add831c4759,edb14823-7f51-4530-97a5-db458a40b9be,2f333c6c-c57e-44f0-a43e-e7f397e31051,4a863c04-bc9b-405e-ab77-e960b58eac1b/parameters/vCar:Chassis,sLap:Chassis/data/aggregate
```

CSV Result
```
time,tagValues,Mean(vCar:Chassis),Mean(sLap:Chassis)
1563027942645000000,InfluxDbUri=http://influxdb1/;DatabaseName=Driver_A;MeasurementName=Measurement_A,128.790105799527,1852.37860522758
```

JSON result
```Json
{
    "timestamps": [
        1563027942645000000
    ],
    "values": {
        "Mean(vCar:Chassis)": [
            128.79010579952748
        ],
        "Mean(sLap:Chassis)": [
            1852.37860522758
        ]
    },
    "tags": [
        {
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Driver_A",
            "MeasurementName": "Measurement_A"
        }
    ]
}
```

#### Querying using search criteria to find the sessions

Endpoint
```
GET api/v1/connections/{connection name}/sessions/parameters/{parameter_1,parameter_2,...,parameter_n}/data/aggregate
```

Example
```
GET api/v1/connections/Connection/sessions/parameters/vCar:Chassis,sLap:Chassis/data/aggregate?from=1563027942645000000&to=1563028393209000000
```

CSV Result
```
time,tagValues,Mean(vCar:Chassis),Mean(sLap:Chassis)
1563027942645000000,InfluxDbUri=http://influxdb2/;DatabaseName=Driver_B;MeasurementName=Measurement_B,193.745431884418,2482.41148899907
```

JSON result
```Json
{
    "timestamps": [
        1563027942645000000
    ],
    "values": {
        "Mean(vCar:Chassis)": [
            193.74543188441757
        ],
        "Mean(sLap:Chassis)": [
            2482.4114889990706
        ]
    },
    "tags": [
        {
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Driver_A",
            "MeasurementName": "Measurement_A"
        }
    ]
}
```

#### A more complete example

* Get maximum and minimum for parameters gLat and gLong
  * for sessions
    * where the number of laps is greater than 5
    * between 05/09/2019 and 08/09/2019,
    * where driver is Driver_A, 
  * where
    * lapnumber is 3
    * vCar is greater than 50
  * group by sessionId 

Query
```
GET /api/v1/connections/Connection/sessions/parameters/gLat;max,gLat;min,gLong;max,gLong;min/data/aggregate?filter=lapsCount;gt;5,timeOfRecording;gt;2019-09-05,timeOfRecording;le;2019-09-08&details=Driver:Driver_A&dataFilter=vCar;gt;50&tagFilter=lapnumber;eq;3&groupby=sessionId 
```

The results below will be grouped by session id.

CSV Result
```
time,tagValues,Max(gLat),Min(gLat),Max(gLong),Min(gLong)
0,sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,35.095275,-37.75869,12.1744543381271,-42.8521866141287
0,sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,35.48277,-39.3381,12.3903205832789,-41.8402346784472
```

JSON result
```Json
{
    "timestamps": [
        0,
        0
    ],
    "values": {
        "Max(gLat)": [
            35.095275,
            35.48277
        ],
        "Min(gLat)": [
            -37.75869,
            -39.3381
        ],
        "Max(gLong)": [
            12.174454338127124,
            12.390320583278896
        ],
        "Min(gLong)": [
            -42.852186614128726,
            -41.840234678447182
        ]
    },
    "tags": [
        {
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        }
    ]
}
```

#### More grouping

If we remove the restriction for the lap number to be 3, i.e. all lap numbers count, and add the lap number to the grouping criteria this is what you get.

Query
```
GET /api/v1/connections/Connection/sessions/parameters/gLat;max,gLat;min,gLong;max,gLong;min/data/aggregate?filter=lapsCount;gt;5,timeOfRecording;gt;2019-09-05,timeOfRecording;le;2019-09-08&details=Driver:Driver_A&dataFilter=vCar;gt;50&groupby=sessionId&groupby=lapnumber 
```

CSV Result
```
time,tagValues,Max(gLat),Min(gLat),Max(gLong),Min(gLong)
0,lapnumber=0;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,33.72678,-33.4521,11.8586146127512,-37.2143195237094
0,lapnumber=0;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,38.430675,-37.60173,12.7337632155516,-43.6732321910843
0,lapnumber=10;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,37.165185,-39.36753,11.5722874015463,-43.2134811128327
0,lapnumber=11;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,34.96284,-39.83841,12.5117407547076,-44.9835696000115
0,lapnumber=12;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,35.82612,-40.53492,12.3324144333545,-41.0000822642959
0,lapnumber=13;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,37.69002,-36.517725,13.0360252262149,-43.1284166382862
0,lapnumber=14;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,37.38591,-27.11484,12.5916981787597,-41.8235044328597
0,lapnumber=15;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,14.95044,-2.212155,8.19101150870976,-29.7182215726177
0,lapnumber=2;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,33.231375,-33.17742,12.344952290265,-42.4415686197203
0,lapnumber=2;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,33.19704,-39.588255,12.3729186960198,-38.9705636985105
0,lapnumber=3;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,35.095275,-37.75869,12.1744543381271,-42.8521866141287
0,lapnumber=3;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,35.48277,-39.3381,12.3903205832789,-41.8402346784472
0,lapnumber=4;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,34.51158,-38.175615,11.8119461800276,-44.7994001032083
0,lapnumber=4;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,37.38591,-41.202,14.0960948009828,-42.2152442330664
0,lapnumber=5;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,35.929125,-32.00022,11.6555463565051,-44.4988815467869
0,lapnumber=5;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,37.645875,-41.11371,13.0937867904175,-39.3710973676435
0,lapnumber=6;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,33.486435,-26.442855,12.1509661821291,-46.5357704123979
0,lapnumber=6;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,34.95303,-33.4521,12.9584305867619,-39.1888394045471
0,lapnumber=7;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,34.310475,-32.20623,12.2504287569192,-37.0345506377725
0,lapnumber=7;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,34.241805,-36.35586,12.422661772807,-39.741736039877
0,lapnumber=8;sessionId=73e259df-ea93-4e8f-baf8-3e0d70346e8b;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,19.291365,-1.45188,5.2690285489107,-36.5473712072457
0,lapnumber=8;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,37.21914,-36.65997,13.8111222001655,-43.7717189282954
0,lapnumber=9;sessionId=f246cfe1-fe94-478c-9032-622728f5983c;InfluxDbUri=http://influxdb1/;DatabaseName=Database;MeasurementName=Measurement,36.233235,-37.665495,14.370239110301,-40.0928613002968

```

JSON result
```Json
{
    "timestamps": [
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
    "values": {
        "Max(gLat)": [
            33.726780000000005,
            38.430674999999994,
            37.165184999999994,
            34.962840000000007,
            35.826119999999996,
            37.69002,
            37.38591,
            14.95044,
            33.231375,
            33.19704,
            35.095275,
            35.48277,
            34.511579999999995,
            37.38591,
            35.929125,
            37.645875000000004,
            33.486435,
            34.95303,
            34.310475000000004,
            34.241805,
            19.291365,
            37.219139999999996,
            36.233235
        ],
        "Min(gLat)": [
            -33.4521,
            -37.601729999999996,
            -39.36753,
            -39.83841,
            -40.53492,
            -36.517725,
            -27.114840000000004,
            -2.212155,
            -33.17742,
            -39.588255,
            -37.75869,
            -39.3381,
            -38.175615000000008,
            -41.202,
            -32.00022,
            -41.113710000000005,
            -26.442855,
            -33.4521,
            -32.20623,
            -36.35586,
            -1.45188,
            -36.65997,
            -37.665495
        ],
        "Max(gLong)": [
            11.85861461275122,
            12.733763215551592,
            11.572287401546321,
            12.51174075470762,
            12.332414433354476,
            13.036025226214866,
            12.591698178759685,
            8.19101150870976,
            12.34495229026502,
            12.372918696019829,
            12.174454338127124,
            12.390320583278896,
            11.811946180027579,
            14.09609480098278,
            11.655546356505081,
            13.093786790417479,
            12.150966182129059,
            12.958430586761864,
            12.25042875691922,
            12.422661772806979,
            5.2690285489107005,
            13.811122200165505,
            14.370239110301
        ],
        "Min(gLong)": [
            -37.21431952370942,
            -43.673232191084296,
            -43.213481112832717,
            -44.983569600011535,
            -41.000082264295877,
            -43.128416638286232,
            -41.823504432859743,
            -29.718221572617658,
            -42.441568619720286,
            -38.970563698510468,
            -42.852186614128726,
            -41.840234678447182,
            -44.799400103208256,
            -42.215244233066436,
            -44.498881546786926,
            -39.371097367643458,
            -46.535770412397881,
            -39.188839404547124,
            -37.034550637772462,
            -39.741736039877004,
            -36.547371207245661,
            -43.7717189282954,
            -40.092861300296839
        ]
    },
    "tags": [
        {
            "lapnumber": "0",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "0",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "10",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "11",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "12",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "13",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "14",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "15",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "2",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "2",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "3",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "3",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "4",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "4",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "5",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "5",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "6",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "6",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "7",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "7",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "8",
            "sessionId": "73e259df-ea93-4e8f-baf8-3e0d70346e8b",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "8",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        },
        {
            "lapnumber": "9",
            "sessionId": "f246cfe1-fe94-478c-9032-622728f5983c",
            "InfluxDbUri": "http://influxdb1/",
            "DatabaseName": "Database",
            "MeasurementName": "Measurement"
        }
    ]
}
```