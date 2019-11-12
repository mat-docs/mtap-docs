# ![logo](/Media/branding.png) Telemetry Analytics API

### Table of Contents
- [**Introduction**](../README.md)<br>
- [**Installation**](Installation.md)<br>
- [**Getting started**](GettingStarted.md)<br>
- [**Authorization**](Authorization.md)<br>
- [**Querying Metadata**](Metadata.md)<br>
- [**Consuming Data**](ConsumingData.md)<br>
- [**Session Versions**](SessionVersions.md)<br>
- **Views**<br>


# Views

A user can specify the view of parameters and then use it to query parameters data without specifying all parameters every time in the url when consuming data. 

## Get all views

Example
```
GET api/v1/views
```
Result
```json

 [
    {
        "Id": 2,
        "Name": "TestView",
        "Description": "Some test parameters."
    }
]
```
## Query view parameters

Request
```
GET api/v1/views/2/parameters
```

Result
```json
[
    "NGear:Chassis",
    "vCar:Chassis"
]
```


# CRUD API for Views

API exposes a CRUD API for views. You can use this CRUD in order to create, modify or delete "views".

## Add new view

Request
```
POST api/v1/views
```

Body
```json
{
    "Name": "TestView",
    "Description": "Some test parameters."
}
```
## Edit view

Request
```
PUT api/v1/views/
```
Body
```json
{
    "Id" : 2,
    "Name": "TestView",
    "Description": "Some test parameters."
}
```

## Delete view

Request
```
DELETE api/v1/views/2
```

## Add view parameters

Request
```
PUT api/v1/views/2/parameters
```

Body
```json
[
    "NGear:Chassis",
    "gLat:Chassis"
]
```

Result
```json
[
    "NGear:Chassis",
    "gLat:Chassis",
    "vCar:Chassis"
]
```
## Delete view parameters

Request
```
DELETE api/v1/views/2/parameters
```
Body
```
[
    "NGear:Chassis"
]
```
Result
```json
[
    "gLat:Chassis",
    "vCar:Chassis"
]
```

# Consuming Data using a View

Consuming data in views is as easy as replacing the list of parameters of the session in the common Url Mask for consuming data with the name of the View that we want to use. See [Consuming data](/docs/ConsumingData.md) for extended information.

Mask
```
GET api/{apiVersion}/connections/{connection friendly name}/sessions/{sessionKey}/view/{view name}/{frequency}/data
```

Example
```
GET api/v1/connections/SQLRACE01/sessions/016fa61e-33e2-7e85-1bc9-4ab56c668136/view/TestView 2/10/data?filter=vCar:Chassis;gt;326
```

Result
```json
[
    {
        "Time": "15:03:13.3810000",
        "Values": {
            "gLat:Chassis": 1.9057499999999998,
            "NGear:Chassis": 8,
            "vCar:Chassis": 326.445
        }
    },
    {
        "Time": "15:03:13.4810000",
        "Values": {
            "gLat:Chassis": 1.7905000000000004,
            "NGear:Chassis": 8,
            "vCar:Chassis": 326.481
        }
    },
    {
        "Time": "15:03:13.6810000",
        "Values": {
            "gLat:Chassis": 0.13,
            "NGear:Chassis": 8,
            "vCar:Chassis": 326.08000000000004
        }
    }
]
```

## Multiple sessions query

All data views and parameters specifing is available with multi session
query.

Example
```
GET api/v1/connections/SQLRACE01/sessions/
26c9ed85-e7ab-0e3f-0fc0-8a69f7743883,0095fa36-a3cb-1dc0-59c0-df620ed271db,
21c9e13c-bed0-b738-b067-33e1b67433ea,6a463017-933c-9e2a-99d7-8e0d9f1d6049,
92cbbae9-cd2c-aff8-f071-856bc116405a
/view/test4/10/data/count?filter=vCar:Chassis;gt;300
```

Result
```json
{
    "26c9ed85-e7ab-0e3f-0fc0-8a69f7743883": 0,
    "0095fa36-a3cb-1dc0-59c0-df620ed271db": 210,
    "21c9e13c-bed0-b738-b067-33e1b67433ea": 0,
    "6a463017-933c-9e2a-99d7-8e0d9f1d6049": 0,
    "92cbbae9-cd2c-aff8-f071-856bc116405a": 0
}
```

