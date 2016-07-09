## Synopsis

Simplifies SQL database querying a bit

## Code Example

SqlClient creation:

```cs
var sqlClient = new SqlClient(regularConnectionString);
```

ExecuteNonQueryAsync:
```cs
var rowsAffected = await sqlClient.ExecuteNonQueryAsync(
	"INSERT INTO dbo.Users (Username) VALUES (@username)",
	new Dictionary<string, object>
	{
		{ "@username", "Vasile Pupkeanu" }
	});
```

ExecuteReaderAsync:
```cs
var userList = await sql.ExecuteReaderAsync(
	"SELECT Id, Username FROM dbo.Task WHERE Id = @id",
	new Dictionary<string, object>
	{
		{ "@id", userId },
	},
	reader =>
	{
		var result = new List<User>();
		while (reader.Read())
		{
			result.Add(new User
			{
				Id = reader.GetString(0),
				Username = reader.GetString(1)
			});
		}
		return result;
	});
```

## Motivation

Syntax sugar is syntax sugar: it's not a necessary thing per se but it can improve code quality.

## Installation

Prosecco is a available in a form of a NuGet package.
Follow regular installation process to bring it to your project.
https://www.nuget.org/packages/Prosecco/

## API Reference

// TODO

## Tests

There are no automated tests for the project at the moment.

## License

The code is distributed under the MIT license.