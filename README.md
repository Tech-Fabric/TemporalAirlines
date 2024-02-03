# Introduction 

This repository provides code used for short demonstration of Temporal basic concepts and features applied to business cases

# Usage

Prerequisites:

* .NET 8
* Temporal server running

# Build and Test

Build the Project

Build:

    dotnet build

Run Temporal Cluster + API (from temporal-server folder):

    docker compose up

Run tests:

    dotnet test

Run API:

    dotnet TemporalAirlinesConcept.Api.dll

# Links 

* [API](http://localhost:5222/swagger) 
* [Temporal UI](http://localhost:8080/)
* [Temporal Server](http://localhost:7233/)
* [Cosmos DB explorer](https://localhost:8081/_explorer/index.html)

# Environment

You can launch Temporal Cluster + API with Docker Compose using IDE + docker-compose project or manually from temporal-server folder using docker compose up command

In docker-compose API service could be commented and API could be launched separately. Change the connection strings in this case.

Alternatively, you can launch an environment for this code using a local developemnt environment using  
[these instructions](https://learn.temporal.io/getting_started/typescript/dev_environment/#set-up-a-local-temporal-development-cluster)