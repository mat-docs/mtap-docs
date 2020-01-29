## What is TAP Helm Charts

[TAP Helm charts](https://github.com/mat-docs/mtap-docs/blob/master/Helm/README.md) allows you to define and deploy a functional TAP platform into the Kubernetes cluster with just an easy configuration file and a command-line.

Helm Charts helps you define, install, and upgrade even the most complex Kubernetes application. 

## Folders structure

- /builds: Contains Azure DevOps yaml files to deploy Helm Charts to different artifacts repositories like Artifactory or Azure DevOps pipelines.
- /mat.tap.requisites: Contains the Helm charts needed to deploy the pre-requisites of a mat.tap.services deployment. Storage services like Mssql or InfluxDB are defined here.
- /mat.tap.services: Contains the Helm charts needed to deploy all TAP services in Kubernetes.

## Pre-requisites

You will need a Kubernetes cluster and some basic knowledge about Kubernetes and Helm to be able to use these Helm charts.

Apart from this, the TAP platform requires some external services to run properly:

- Apache Kafka: Kafka is used as the message broker of our services. You will need to install an instance of Kafka separately and make it accessible from the Kubernetes cluster to be able to run the TAP platform.

- InfluxDB database: One or more InfluxDB time-series databases to persist and query Telemetry data in the platform. The default Helm chart configuration uses a unique instance of this database but you can override easily the rest of the services to set up more than one InfluxDB instances. You can use your InfluxDB instances from outside the cluster or you can use the provided `mat.tap.requisites` helm chart to deploy several InfluxDB instances into the Kubernetes cluster.

- SQL Server database: TAP uses relational database to persist service configurations and sessions metadata. The default Helm chart configuration uses a unique instance of this database. You can use your database instance from outside the cluster or you can use the provided `mat.tap.requisites` helm chart to deploy an instance into the Kubernetes cluster.

## Persistence volumes requirements

Some of the services use Persistence Volumes in Kubernetes. These services expect a "Persistence Volume Claim" with a specific name. These volumes have to be created and managed manually by a Kubernetes administrator.

This is the list of claims needed for all the services with storage needs:

| Persistence Volume Claim | Chart | Service | Description |
|-|:--:|:-:|-|
| dependency-service-data | mat.tap.services | dependency-service | Dependencies service persistence volume for Sessions metadata when using Advanced Streams protocol.  |
| mssql-data | mat.tap.requisites | mssql | SQL Server database persistence volume for Sessions metadata and Configuration. |
| influxdb1-data | mat.tap.requisites | influxdb | InfluxDb persistence volume. If you use multiple instances of InfluxDb (numInstances > 1) you will need a persistence volume for each one (influxdb1-data, influxdb2-data, ...) |


## Installation and Deployment

``` bash
helm upgrade --install -f {my-custom-values.yaml} {deployment name} {url|path of the helm chart}
```

example 1: 

``` bash
helm upgrade --install -f ./your-values-tap-services.yaml tap-services ./mat.tap.services`
```

example 2: 

``` bash
helm upgrade --install -f ./your-values-tap-requisites.yaml tap-requisites ./mat.tap.requisites`
```

## Delete a deployment

``` bash
helm delete tap-services --purge
```

## Values YAML for "mat.tap.services"

Create `values.yaml` file to define the parameters of the cluster deployment. These values override the default values defined in `mat.tap.services` chart. All the services of the platform can be configured through this file. Pass this `values.yaml` to the Helm chart deployment command line (like the example above).

### Global variables

There is a Global section in the values yaml file to define common parameters to all the services of the TAP platform. Some of these parameters are essential to be able to run our platform, like Kafka broker, relational database or InfluxDB url.

Example:

``` bash
global:
    namespace: default
    publicDomain: your-public-domain.com
    database:
        password: password
    kafka:
        brokerList: 127.0.0.1
    loadBalancer: 
        type: traefik           ## valid values = "traefik", "alb"
        gateway: gateway        ## used in traefik type
```


The following tables list all the possible Global parameters of the TAP chart and their default values.

| Parameter                                  | Default                               | Required / Optional  | Description
| ------------------------------------------ | ------------------------------------- | :--------------------: | ---------------------------------------------------
| `namespace`                                | `default`                             | Required             | Kubernetes namespace to deploy the services
| `publicDomain`                             | `your-public-domain.com`              | Required             | Public domain for exposed services of the cluster
| `database.hostname`                        | `mssql`                               | Optional             | Hostname of the common relational database. Pointing by default to `mssql` the Mssql installed through Requisites chart.
| `database.user`                            | `sa`                                  | Optional             | Relational database user name
| `database.password`                        | `password`                            | Required             | Relational database password
| `kafka.brokerList`                         | `127.0.0.1`                           | Required             | Broker list of Kafka cluster. Use ',' to separate multiple values.
| `influxDb.url`                             | `http://influxdb1`                    | Optional             | Common InfluxDB url. Pointing to  `influxdb1` InfluxDb installed through requisites chart by default.
| `loadBalancer.type`                        | `traefik`                             | Optional             | Type of Load balancer to use for exposed services. Valid values are `traefik` and `alb` (Application Load Balancer).
| `loadBalancer.gateway`                     | `gateway`                             | Optional             | Gateway name for Traefik load balancer configuration.
| `containerRegistry.type`                   | `ecr`                                 | Optional             | Type of container registry to use when Kubernetes pull the docker images of the services. Valid values are `ecr` (Amazon ECR) and `bintray` (Jfrog Bintray).
| `containerRegistry.username`               | `user`                                | Required in Bintray  | Username to login in Bintray container registry.
| `containerRegistry.password`               | `password`                            | Required in Bintray  | Password to login in Bintray container registry.
| `licensing.customerId`                     | `00000000-0000-0000-0000-000000000000`| Required             | Licence customer identifier
| `licensing.licenseServerUri`               | `https://localhost/mockSentinelEMS`   | Required             | Licence Server address where to get the licence from
| `release.version`                          | `{last-mtap-release-version}`         | Optional             | Version of MTAP to deploy.

### Container registries

We have two types of container registry available to use with this version of MTAP Helm charts, `Amazon ECR` and `Bintray JFrog`. `Amazon ECR` is used for internal MAT projects and developing environment and `Bintray` is used for external users. Each container registry has its own specific authentication requirements.

#### Authentication with Amazon ECR

Pre-requisite: You must use a Kubernetes deployment in AWS to be able to use this type of container registry.

You must make a manual configuration step in our MAT Amazon ECR to enable your Kubernetes Amazon account. After this step, you will not need more additional authentication to use this type of container registry.

This is the default container registry if you don't specify any container parameter in your global variables.

#### Authentication with JFrog Bintray

You will need a user in JFrog Bintray with access to our MTAP bintray repository to be able to use this container registry.

In this case password is replaced by the API Key generated for your BinTray user. In order to get this API Key, browse to: Your profile -> API Key -> Enter password (Submit)

After that, you must use this API Key for the `containerRegistry.password` parameter in your `values.yaml` file.

example:

``` bash
    containerRegistry: 
        type: bintray
        username: your.username@mclarenappliedtechnologies
        password: e735cffb57e9e6e94e5dd3d6ca71408880d9aab7
```

### TAP Services

This is the complete list of services deployable through `mat.tap.services` charts:


| Service | Description | Documentation |
|--------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| dependency-service | Dependencies service facilitates managing reference metadata for Advanced Streams.  | [Dependencies service documentation](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/115003531373-API-Reference-Dependencies-Service) |
| topic-management | Topic Management is a REST service to create, edit and delete Advanced Streams compatible Kafka topics. | [Topic Management service documentation](https://mclarenappliedtechnologies.zendesk.com/hc/en-us/articles/115003539194-API-Reference-Topic-Management-Service) |
| identity-service | Identity Service implements OpenID Connect and provides authentication services for Telemetry Analytics API and other services in MTAP. | [Identity service documentation](https://github.com/McLarenAppliedTechnologies/mat.tap.query.api/blob/master/docs/IdentityServer.md) |
| replay-service | Replay Service allows the replay of historical sessions from TAP API as if they were live again. | [Replay service documentation](https://github.com/mat-docs/mtap-docs/blob/master/ReplayService/README.md) |
| tapi | Telemetry Analytics API service provides programmatic access to persisted stream data. This REST API provides access to session metadata, telemetry data and model results | [Telemetry Analytics API documentation](https://github.com/mat-docs/mtap-docs/blob/master/TAPApi/README.md) |
| influx-writer | InfluxDb writer service is responsible for persisting telemetry data, session metadata and model execution outputs. | [InfluxDb writer documentation](https://github.com/mat-docs/mtap-docs/blob/master/InfluxWriter/README.md) |
| signalr | SignalR is a websocket push mechanism based live service that allows multiple clients to subscribe to various parameters for live data and get pushed data streams | [SignalR service documentation](https://github.com/mat-docs/mtap-docs/blob/master/SignalR/README.md) |
| parameter-mapping | Parameter Mapping allows to rename parameters of the live sessions running in the platform | [Parameter Mapping service documentation](https://github.com/mat-docs/mtap-docs/blob/master/ParameterMapping/README.md) |

All TAP services are disabled by default and each service has to be enabled explicitly in the yaml file with the common parameter `enabled`. 

In the following example we are deploying "Dependency service" and "Identity service":

``` bash
dependency-service:
    enabled: true

identity-service:
    enabled: true
```

### Public access

If the service needs to be accessible from outside of Kubernetes cluster then it can be specified using the parameter `public`. This parameter is common to all services. By default this parameter is disabled and the access from the outside of cluster is not allowed:

In the following example we are deploying "Influx writer" with public access:

``` bash
influx-writer:
    enabled: true
    public: true
```

The url to access the services from outside the cluster is formed with the name of the service and the global parameters `namespace` and `publicDomain` with the following convention:

``` bash
https://{namespace}-{service-name}.{publicDomain}
```

Using the previous example the Influx writer service public url would be:

``` bash
https://default-influx-writer.your-public-domain.com
```

### Versions

This Helm charts are deploying the version of TAP services of the corresponding release version, by default the latest released version of TAP. You can deploy a specific old version using the global variable `release.version`.

example:

``` bash
global:
    release:
        version: 2019.2
```

If you are using MAT Amazon ECR container registry you can deploy a specific version of each service using the common variable `image.tag`. For instance, in the following example we are deploying the version `0.11.0-19303.9` of the Replay service:

``` bash
replay-service:
    image:
        tag: 0.11.0-19303.9
```

### Configuration parameters for each service

Most of the services have enough default values defined in Helm charts to be deployable without extra configuration. All the services have some configuration that can be overridden through the values YAML file. You can find a quick example of these parameters looking at `values.yaml` file in the corresponding folder of each service chart. The values file defines the default deployment parameters of the service.

If you need more information about some of the configuration parameters, you can query the corresponding documentation of the service. Most of the parameters have a direct correspondence with `Environment Variables` parameters or `JSON configuration` file of the service. It is important to mention that some of the services have specific details of the chart configuration which are not obvious by looking only at the service Documentation.

Below you can find specific information about some of these services.

#### Configuring Telemetry Analytics API (tapi)

Telemetry Analytics API service has his own REST API to manage the connections and configurations to InfluxDB and SQL Server, but sometimes it is easier to set this configuration through Helm charts because some of the parameters are completed automatically in an implicit way.

This service allows seeding of the configuration database with `seedOption` and `seedData` parameters.
 
An example to seed the database with connections:


``` bash
tapi:
    enabled: true
    public: true
    database:
        seedOption: always # "" = do not seed, "always" = always seed, "empty" = seed if database is empty.
        seedData:
            - identifier: "Connection 1"
              sessionsDatabase: "Connection1_Metadata"
              influxDbDetails:
                - topicName: "Topic1"
                  label: "0"
                  database: "InfluxDatabase"
                  influxDbUrl: "http://influxdb1"
                  measurement: "Measurement1"
                - topicName: "Topic1"
                  label: "1"
                  database: "InfluxDatabase"
                  influxDbUrl: "http://influxdb2"
                  measurement: "Measurement1"
            - identifier: "Connection 2"
              sessionsDatabase: "Connection2_Metadata"
              influxDbDetails:
                - topicName: "Topic2"
                  label: "*"
                  database: "InfluxDatabase"
                  influxDbUrl: "http://influxdb1"
                  measurement: "Measurement2"
```

You can find more extensive documentation in [Telemetry Analytics API documentation](https://github.com/mat-docs/mtap-docs/blob/master/TAPApi/README.md).

#### Configuring InfluxDB writer service

You can create more than one instance of this service through this Helm charts using the parameter `instances`. You must specify at least one instance name through values.yaml configuration to use this service.

``` bash
influx-writer:
    enabled: true
    public: false
    instances:
        - name: K1
        - name: K2
```

Influx writer service has its own REST API to manage the connections and configurations to InfluxDB and SQL Server, but sometimes it is easier to set this configuration through Helm charts because some of the parameters are completed automatically in an implicit way.

This service allows seeding of the configuration database with `seedOption` and `seedData` parameters.
 
``` bash
influx-writer:
    enabled: true
    public: false
    instances:
        - name: K1
          initializeDatabase: true
          seedOption: always # "" = do not seed, "always" = always seed, "empty" = seed if database is empty.
          seedData:
              name: "Kafka connection"
              description: "Streaming settings to use Kafka"
              topics:
                - topic: "Topic1"
                  description: "Topic used to save data from Kafka"
                  sessionsDatabase: "Connection1_Metadata"
                  influxDbConnections:
                    - label: "0"
                      influxDbUrl: "http://influxdb1"
                      database: "InfluxDatabase"
                      measurement: "Measurement1"
                - topic: "Topic1"
                  description: "Topic used to save data from Kafka"
                  sessionsDatabase: "Connection1_Metadata"
                  influxDbConnections:
                    - label: "1"
                      influxDbUrl: "http://influxdb2"
                      database: "InfluxDatabase"
                      measurement: "Measurement1"
        - name: K2
          initializeDatabase: true
          seedOption: always # "" = do not seed, "always" = always seed, "empty" = seed if database is empty.
          seedData:
              name: "Kafka connection"
              description: "Streaming settings to use Kafka"
              topics:
                - topic: "Topic2"
                  description: "Topic used to save data from Kafka"
                  sessionsDatabase: "Connection2_Metadata"
                  influxDbConnections:
                    - label: "*"
                      influxDbUrl: "http://influxdb1"
                      database: "InfluxDatabase"
                      measurement: "Measurement2"
```

You can find more extensive documentation in [InfluxDb writer documentation](https://github.com/mat-docs/mtap-docs/blob/master/InfluxWriter/README.md).

#### Configuring SignalR service

SignalR service needs a list of Kafka topic names as an input configuration to be able to run properly in TAP platform. Use `topicNames` parameter to define this list of topics:

``` bash
signalr:
    enabled: true
    public: true
    topicNames: 
      - Topic1
      - Topic2
```

You can find more extensive documentation in [SignalR service documentation](https://github.com/mat-docs/mtap-docs/blob/master/SignalR/README.md).

#### Configuring Parameter Mapping service

Parameter mapping service uses lists of input, output topics in addition to source and target parameter pairs to define the renaming. You should define the same number of input and output topics using `inputTopics` and `outputTopics` as they're paired by index. Define parameter renaming using `sourceIdentifier` and `targetIdentifier`.

You can create more than one instance of this service through Helm charts using the parameter `instances`. You must specify at least one instance name through `values.yaml` configuration to use this service.

``` bash
parameter-mapping:
    enabled: true
    instances:
        - name: Map1
          configuration:
            inputTopics: 
                - Topic1
                - Topic2
            outputTopics: 
                - MappendTopic1
                - MappendTopic2
            parameters: 
                - sourceIdentifier: ParameterIdentifier1
                  targetIdentifier: RenamedParameterIdentifier1
                - sourceIdentifier: ParameterIdentifier2
                  targetIdentifier: RenamedParameterIdentifier2
```

You can find more extensive documentation in [Parameter Mapping service documentation](https://github.com/mat-docs/mtap-docs/blob/master/ParameterMapping/README.md).


## Values YAML for "mat.tap.requisites"

Create `values.yaml` file to define the parameters of the cluster deployment. These values override the default values defined in `mat.tap.requisites` chart. All the services of the platform can be configured through this file. Pass this `values.yaml` to the Helm chart deployment command line (like the example in "Installation and Deployment").

### Global variables

There is a Global section in the `values.yaml` file to define common parameters to all the requisites.

Example:

``` bash
global:
    namespace: default
    publicDomain: your-public-domain.com
```

The following tables list all the possible Global parameters of the TAP requisites chart and their default values.

| Parameter                                  | Default                               | Required / Optional    | Description
| ------------------------------------------ | ------------------------------------- | :--------------------: | ---------------------------------------------------
| `namespace`                                | `default`                             | Required               | Kubernetes namespace to deploy the services
| `publicDomain`                             | `your-public-domain.com`              | Required               | Public domain for exposed services of the cluster

### TAP Requisites

These charts help you to install some requisites services for TAP platform. You can use these Helm deployment or use your own services. This is the complete list of services deployable through this `mat.tap.requisites` charts:

| Service | Description | Documentation |
|-|-|-|
| mssql | SQL Server running on Linux based on Ubuntu 16.04.  | [SQL Server docker documentation](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-configure-environment-variables?view=sql-server-ver15#use-with-docker) |
| influxdb | InfluxDB time series database built from the ground up to handle high write and query loads. | [InfluxDb docker documentation](https://hub.docker.com/_/influxdb) |

In the following example we are deploying SQL Server and two instances of InfluxDB:

``` bash
mssql:
    password: your-sa-password

influxdb:
    numInstances: 2
```
