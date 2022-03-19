# DbConnectionWrapper

A simple wrapper for IDbConnection that allows logging SQL statements and works easily with Dapper or anything where you can pass the IDbConnection you want to use.

It has support for Stored Procedures, Functions, and Table-Valued Functions and formats the logs such that they can be directly run in SSMS which is extremely useful for debugging.

All the code is in the `DbConnectionWrapper.cs` class, but I'm working on adding a Nuget package.

## Usage

``` C#
// #r "nuget: Dapper"
// #r "nuget: System.Data.SqlClient"
using var conn = new WrappedDbConnection(
    new SqlConnection(@"Server=.\SqlExpress;Database=Database;Integrated Security=SSPI"), 
    x => new ActionDbCommand(x, Console.WriteLine)
);

// Example: int
var result = conn.Query("SELECT * FROM Table WHERE Id=@id", new
{
    Id = 1,
});

/*
DECLARE @Id Int = '1'
SELECT * FROM Table WHERE Id=@id
*/

// Example: string
var result = conn.Query("SELECT * FROM Table WHERE Name=@name", new
{
    name = "github"
});

/*
DECLARE @name NVarChar(4000) = 'github'
SELECT * FROM Table WHERE Name=@name
*/

// Example: sql function
var result = conn.Query(@"SELECT dbo.SomeFunction(@Id, @Name)", new
{
    Id = 1,
    Name = (string?)null,
});

/*
DECLARE @Id Int = '1'
DECLARE @Name NVarChar(1) = NULL
SELECT dbo.SomeFunction(@Id, @Name)
*/

// Example: stored procedure
var result = conn.Query(@"EXEC StoredProc @Id", new
{
    Id = 1,
});

/*
DECLARE @Id Int = '1'
EXEC StoredProc @Id
*/

// Example: table-valued function
var result = conn.Query(@"SELECT * FROM TableValuedFunction(@Id,@Date,@Format)", new
{
    Id = 1,
    Date = DateTime.Parse("2/1/2016"),
    Format = "MMM-yy"
});

/*
DECLARE @Id Int = '1'
DECLARE @Date DateTime = '2/1/2016 12:00:00 AM'
DECLARE @Format NVarChar(4000) = 'MMM-yy'
SELECT * FROM TableValuedFunction(@Id,@Date,@Format)
*/

// Example: output parameters
var param = new DynamicParameters(new
{
    InputId = 1,
});
param.Add("OutputId", dbType: DbType.Int32, direction: ParameterDirection.Output);
var result = conn.Query(@"EXEC StoredProc @InputId,@OutputId OUTPUT", param);

/*
DECLARE @InputId Int = '1'
DECLARE @OutputId Int = NULL
EXEC StoredProc @InputId,@OutputId OUTPUT
SELECT @RunId AS [RunId]
*/

// Example: return value
var param = new DynamicParameters();
param.Add("Return", dbType: DbType.Int32, direction: ParameterDirection.ReturnValue);
conn.Execute(@"Proc", param, commandType: CommandType.StoredProcedure);
var result = param.Get<int>("Return");

/*
DECLARE @Return Int = NULL
EXEC @Return = Proc
SELECT @Return AS [Return]
*/

```
