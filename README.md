# Azure CosmosDB Cassandra .NET Core sample app

This is a sample app written in .NET Core 2.2 that connects to Azure CosmosDB via the Cassandra driver.

The app demonstrates inserting and reading a simple table.

The app has been tested on Windows 10, Ubuntu 18.04 and Azure Cloud Shell (bash)

Installation has been tested on Ubuntu 18.04 and Azure Cloud Shell (bash)

## Prerequisites

- Azure Subscription
- Azure Cloud Shell

If you prefer to use your own VM / container, the following are required:

- Azure CLI
- dotnet Core 2.2
- jq (sudo apt-get install jq)

## Installation

### From Azure Cloud Shell (bash)

~~~~
export rg=YourResourceGroupName
export loc=YourLocation
   i.e. centralus
export cname=YourCosmosDBName
   must be unique across CosmosDB as it's part of the DNS name
~~~~

### Create resource group

~~~~
az group create -g $rg -l $loc
~~~~

### Create CosmosDB for Cassandra using defaults

~~~~
az cosmosdb create -g $rg -n $cname --capabilities EnableCassandra
~~~~

### Get the CosmosDB connection key

~~~~
export cpass=$(az cosmosdb list-keys -g $rg -n $cname | jq -r '.primaryMasterKey')
~~~~

### Create the CosmosDB database named myapp

~~~~
az cosmosdb database create -g $rg -n $cname --db-name myapp
~~~~

### Clone the repo

~~~~
git clone https://github.com/bartr/cass-core
cd cass-core
~~~~

### Restore the packages and run the app
~~~~
dotnet restore
dotnet run
~~~~
