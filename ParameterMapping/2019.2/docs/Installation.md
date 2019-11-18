# ![logo](/Media/branding.png) Parameter Mapping Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- **Installation**<br>
  - [*Run with Docker*](#run-with-docker)<br>
  - [*Run as daemon service*](#run-as-a-daemon-service)<br>
- [**Service configuration**](ServiceConfig.md)<br>

## Run with Docker

Parameter Mapping service is shipped as a docker image. For detailed instruction steps, please visit the [docker image hosted on bintray](https://bintray.com/mclarenappliedtechnologies/mtap/parametermapping-service#read).

## Run as a daemon service
### .NET Core runtime dependency
First you need to install .NET Core 2.1 runtime. You can download it [here](https://www.microsoft.com/net/download/dotnet-core/2.1). Example for Ubuntu 18.04 LTE: 

```
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

sudo apt-get --yes install apt-transport-https
sudo apt-get update
sudo apt-get --yes install aspnetcore-runtime-2.1
```

### Daemon installation
<span style="color:red">Please note daemon service installation will be deprecated in future releases in favour of docker.</span>

In the [release bundle](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/sections/115000825753-Downloads) the service is located at `services/MAT.TAP.ParameterMapping`.

In the folder you will find a shell script called `daemon_deploy.sh` to install the service.

before you run it, execute following commands:
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
journalctl --unit MAT.TAP.ParameterMapping.service --follow -n 100
```


and start or stop by 

```
sudo systemctl stop MAT.TAP.ParameterMapping.service
sudo systemctl start MAT.TAP.ParameterMapping.service
```

To configure the service according to the [Service Configuration section](ServiceConfig.md), edit
```
/opt/MAT.TAP.ParameterMapping/appsettings.json
```

**Please note that after you change settings in appsettings.json in opt folder you need to restart service.**

## Logging

Parameter Mapping Service has logging and uses Nlog for logger implementation. You can configure logging in the `nlog.config` file or provide your own logging configuration. More information on available configuration options can be found [here](https://github.com/nlog/nlog/wiki/Configuration-file).
