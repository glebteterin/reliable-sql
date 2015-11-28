ReliableSql - implementation of retry logic for SqlConnection and SqlCommand for handling transient errors.
===========================================================================================================
It works especially good with [Dapper](https://github.com/StackExchange/dapper-dot-net)

Example usage:
--------------

Create an instance of ReliableSqlConnection:
```csharp
var db = new ConnectionManager(connectionString);
var dbConnection = db.CreateConnection();
```

Select data with Dapper:
```csharp
var db = new ConnectionManager(connectionString);
var users = default(List<User>);
db.Execute(cnn => users = cnn.Query<User>("SELECT * FROM Users").ToList());
```