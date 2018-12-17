# cass-core
CosmosDB Cassandra .NET core sample app

CLI Commands

export rg=<<Your Resource Group Name>>
export loc=<<location - i.e. centralus>>
export CNAME=<<Cosmos Name - must be unique across Cosmos as it's part of the DNS name>>

Create resource group (skip this if RG exists)
az group create -g $rg -l $loc

Create CosmosDB for Cassandra using defaults
az cosmosdb create -g $rg -n $CNAME --capabilities EnableCassandra

Get the CosmosDB connection key
export CPASS=$(az cosmosdb list-keys -g $rg -n $CNAME | jq -r '.primaryMasterKey')

Create the CosmosDB database named myapp
az cosmosdb database create -g $rg -n $CNAME --db-name myapp

Clone the repo into your home directory (or wherever)
cd ~
git clone https://github.com/bartr/cass-core
cd cass-core

Restore the packages and run the app
dotnet restore
dotnet run


