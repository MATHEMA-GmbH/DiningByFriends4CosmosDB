# DiningByFriends4CosmosDB

This is a simple example application inspired by the DiningByFriends application in the book Graph Databases in Action (published in Manning)

This application is using the Mathema.Bytecode4CosmosDB nuget package (prerelease: https://www.nuget.org/packages/Mathema.Bytecode4CosmosDB/)

Before you can run the App you have to create a Graph DB in Cosmos DB and modify the following properties accordingly:
```C#
        string cosmosHostname = "Your URL";                               // Enter your Graph DB URL
        int cosmosPort = 443;
        string cosmosAuthKey = "Your auth key";                           // Enter your Authkey here
        string cosmosDatabase = "your database";                          // Enter your Database
        string cosmosCollection = "your colletion";                       // Enter your Colletion  
```

Example 
```C#
        string cosmosHostname = "ddcgremlin.gremlin.cosmosdb.azure.com";
        int cosmosPort = 443;
        string cosmosAuthKey = "... ... ... ... ==";
        string cosmosDatabase = "graphdb";
        string cosmosCollection = "Persons";
```
