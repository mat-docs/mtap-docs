# ![logo](/Media/branding.png) Telemetry Analytics Data Persistence

### Table of Contents
- [**Introduction**](../README.md)<br>
- **Installation**<br>
  - [*Influx DB Installation*](#influxdb-installation)<br>
  - [*Influx Writer prerequisites*](#influxdb-writer-prerequisites)<br>
  - [*Run as standalone*](#run-as-standalone)<br>
  - [*Run as a daemon service*](#run-as-a-daemon-service)<br>
  - [*Influx Writer configuration*](#influx-writer-configuration)<br>
  - [*Logging*](#logging)<br>
- [**Seeding Data**](SeedData.md)<br>
- [**API**](API.md)<br>


## InfluxDb installation
**We strongly recommend deploying InfluxDb writer and InfluxDb on one Linux VM. InfluxDb performs better on Linux and by deploying it on one machine together with service you reduce unnecessary network bandwidth between those services.**

[Download](https://portal.influxdata.com/downloads/) latest v1.x version and install InfluxDb following instructions [here](https://docs.influxdata.com/influxdb/v1.7/introduction/installation/).

**Change index-version in influx.conf (on Ubuntu is located at /etc/influxdb/influxdb.conf)**:

```
# The type of shard index to use for new shards.  The default is an in-memory index that is
# recreated at startup.  A value of "tsi1" will use a disk based index that supports higher
# cardinality datasets.
index-version = "tsi1"
```

### Minimal requirements
One F1 car stream of all samples minimum requirments:  
CPU: 8 cores  
RAM: 16Gb  
Disk: SSD min 15000 IOPS  


## InfluxDb Writer prerequisites

InfluxDb writer subscribes to message brokers and saves telemetry data and session metadata in real-time to time-series and relational databases respectively. InfluxDb Writer is platform-independent and hence can be deployed on Windows or Unix-based systems as a service but **we strongly recommend deploying InfluxDb writer and InfluxDb on one Linux VM. InfluxDb performs better on Linux and by deploying it on one machine together with service you reduce unnecessary network bandwidth between those services.**

### .NET Core runtime
First you need to install .NET Core 2.1 runtime. You can download it [here](https://www.microsoft.com/net/download/dotnet-core/2.1). Example for Ubuntu 18.04 LTE: 

```
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get --yes install apt-transport-https
sudo apt-get update
sudo apt-get --yes install aspnetcore-runtime-2.1
```


## Run as standalone

In order to use InfluxDb writer without running it as a service, add the relevant configuration in `appsettings.Production.json` file and start service using
```
dotnet MAT.TAP.AAS.InfluxDb.Writer.Hosting.dll
```

## Run as a daemon service
<span style="color:red">Please note daemon service installation will be deprecated in future releases in favour of docker.</span>

In the release bundle, which you can [download from here](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/sections/115000825753-Downloads), there is a shell script `services/MAT.TAP.AAS.InfluxDb.Writer/daemon_deploy.sh` for daemon service installation. 

Before you run it, execute following commands:
```
awk 'BEGIN{RS="^$";ORS="";getline;gsub("\r","");print>ARGV[1]}' daemon_deploy.sh
sudo chmod 777 daemon_deploy.sh
```

Then run:
```
./daemon_deploy.sh
```

You can check logs and verify its state with

```
journalctl --unit MAT.TAP.AAS.InfluxDb.Writer.Hosting.service --follow -n 100
```

and start or stop by 

```
sudo systemctl stop MAT.TAP.AAS.InfluxDb.Writer.Hosting.service
sudo systemctl start MAT.TAP.AAS.InfluxDb.Writer.Hosting.service
```

## Influx Writer Configuration
To configure the service, edit
```
/opt/MAT.TAP.AAS.InfluxDb.Writer.Hosting/appsettings.Production.json
```

**Please note that after you change settings in appsettings.Production.json in opt folder you need to restart service.**

A sample configuration and an explanation of settings is given below.
```json
{
  "ConnectionStrings": {
    "HostingDb": "Data Source=MAT-TAP-Influx-Writer-Hosting.db"
  },
  "AllowedOrigins": [
    "*"
  ], 
  "WriterConfiguration": {
    "InitializeDatabase": true, 
    "BatchSize": 10000, 
    "WorkerCount": 5, 
    "BufferSize": 100, 
    "EscapeLineProtocol": false
  },
  "InitializeDatabase": true,
  "SeedOption": "",
  "SeedData": null
}
```
| Property | Description | Suggested Value |
|--|--|--|
| ConnectionStrings.HostingDb | Service database to store streaming and topic configuration. | the default
| AllowedOrigins | CORS Policy configuration. List of origins to allow. Use "*" to allow all origins
| WriterConfiguration | [See below](#writer-configuration) |
| InitializeDatabase | Enable creation and migration of service database defined in `ConnectionStrings.HostingDb` | true |
| SeedOption | [See seeding data section](/docs/SeedData.md) | ""
| SeedData | [See seeding data section](/docs/SeedData.md) | null


#### Writer Configuration
| Property | Description | Suggested Value |
|--|--|--|
| InitializeDatabase| Enable creation and migration of metadata database defined in `TopicConfiguration.SqlConnectionString` | true |
| BatchSize | Max number of lines per stream to write to InfluxDb in a single request. | 10000 |
| WorkerCount | Max number of concurrent workers per stream. | 5 |
| BufferSize | Max number of sample messages per stream to buffer before writing to InfluxDb. | 100 |
| EscapeLineProtocol | Whether to enable line protocol escaping when writing to influx. Suggested to leave false | false |

## Logging

Influx Writer has extensive logging and uses Nlog for logger implementation. You can configure logging in the `nlog.config` file or provide your own logging configuration. More information on available configuration options can be found [here](https://github.com/nlog/nlog/wiki/Configuration-file).
