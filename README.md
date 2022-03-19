# DbConnectionWrapper

A simple wrapper for IDbConnection that allows logging SQL statements and works easily with Dapper or anything where you can pass the IDbConnection you want to use.

## Usage

``` C#
// #r "nuget: System.Data.SqlClient"
using var conn = new WrappedDbConnection(
    new SqlConnection(@"Server=.\SqlExpress;Database=Database;Integrated Security=SSPI"), 
    x => new ActionDbCommand(x, Console.WriteLine)
);

// #r "nuget: Dapper"
var rows = conn.Query("SELECT * FROM [Table]");
```
