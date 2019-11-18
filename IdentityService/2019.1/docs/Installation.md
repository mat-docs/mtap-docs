# ![logo](/Media/branding.png) Identity Service

### Table of Contents
- [**Introduction**](../README.md)<br>
- **Installation**<br>
  - [*Run with Docker*](#run-with-docker)<br>
  - [*Run from terminal*](#run-from-terminal)<br>
  - [*Run as daemon service*](#run-as-a-daemon-service)<br>
- [**Service Config**](ServiceConfig.md)<br>
- [**API**](API.md)<br>


## Run with Docker

Identity service is shipped as a docker image. For detailed instruction steps, please visit the [docker image hosted on bintray](https://bintray.com/beta/#/mclarenappliedtechnologies/mtap/identity-service?tab=readme).

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

### Run from terminal

Alternatively, you can use Identity Service by adding the [relevant configuration](ServiceConfig.md) in `appsettings.Production.json` file and running the following command from the terminal. Make sure you are in the same directory as the **MAT.TAP.IdentityServer.dll**.

    dotnet MAT.TAP.IdentityServer.dll --urls="http://*:5000"

### Daemon installation
<span style="color:red">Please note daemon service installation will be deprecated in future releases in favour of docker.</span>

In the [release bundle](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/sections/115000825753-Downloads) the service is located at `services/MAT.TAP.IdentityServer`.

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
journalctl --unit MAT.TAP.IdentityServer.service --follow -n 100
```


and start or stop by 

```
sudo systemctl stop MAT.TAP.IdentityServer.service
sudo systemctl start MAT.TAP.IdentityServer.service
```

To configure the service according to the [Service Configuration section](ServiceConfig.md), edit
```
/opt/MAT.TAP.AAS.SignalR/appsettings.json
```    