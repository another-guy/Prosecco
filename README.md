## Synopsis

[![Prosecco](https://github.com/another-guy/Prosecco/raw/master/Prosecco.png)](https://github.com/another-guy/Prosecco)

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

ExecuteScalarAsync:
```cs
var userName = await sql.ExecuteScalarAsync(
	"SELECT Name FROM dbo.Task WHERE Id = @id",
	new Dictionary<string, object>
	{
		{ "@id", userId },
	});
```

ExecuteXmlReaderAsync:
```cs
var userList = await sql.ExecuteXmlReaderAsync(
	"SELECT Id, Username FROM dbo.Task WHERE Id = @id",
	new Dictionary<string, object>
	{
		{ "@id", userId },
	},
	xmlReader =>
	{
		var result = new List<User>();
		
		// populate result object from the xmlReader

		return result;
	});
```

## Motivation

Syntax sugar is syntax sugar: it's not a necessary thing per se but it can improve code quality.

## Installation

Prosecco is a available in a form of a NuGet package.
Follow regular installation process to bring it to your project.
https://www.nuget.org/packages/Prosecco/

## Tests

Unit tests are available in Prosecco.Tests project.

## License

The code is distributed under the MIT license.

## Reporting an Issue

Reporting an issue, proposing a feature, or asking a question are all great ways to improve software quality.

Here are a few important things that package contributors will expect to see in a new born GitHub issue:
* the relevant version of the package;
* the steps to reproduce;
* the expected result;
* the observed result;
* some code samples illustrating current inconveniences and/or proposed improvements.

## Contributing

Contribution is the best way to improve any project!

1. Fork it!
2. Create your feature branch (```git checkout -b my-new-feature```).
3. Commit your changes (```git commit -am 'Added some feature'```)
4. Push to the branch (```git push origin my-new-feature```)
5. Create new Pull Request

...or follow steps described in a nice [fork guide](http://kbroman.org/github_tutorial/pages/fork.html) by Karl Broman
