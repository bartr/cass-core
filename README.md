# cass-core

## CosmosDB Cassandra .NET Core sample app

## Installation

From Azure Shell

export rg=<<Your Resource Group Name>>
export loc=<<location - i.e. centralus>>
export cname=<<Cosmos Name - must be unique across Cosmos as it's part of the DNS name>>

Create resource group (skip this if RG exists)
az group create -g $rg -l $loc

Create CosmosDB for Cassandra using defaults
az cosmosdb create -g $rg -n $cname --capabilities EnableCassandra

Get the CosmosDB connection key
export cpass=$(az cosmosdb list-keys -g $rg -n $cname | jq -r '.primaryMasterKey')

Create the CosmosDB database named myapp
az cosmosdb database create -g $rg -n $cname --db-name myapp

Clone the repo into your home directory (or wherever)
cd ~
git clone https://github.com/bartr/cass-core
cd cass-core

Restore the packages and run the app
dotnet restore
dotnet run
