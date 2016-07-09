using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using NSubstitute;
using Prosecco;
using Xunit;

namespace Prosecco.Tests
{
    public class SqlClientTest
    {
        private readonly string connectionString = @"FAKE";
        private readonly SqlClient sut;

        private readonly IDbConnection connection;
        private readonly SqlCommand dbCommand;
        const int nonQueryCommandAffectedRows = 1;

        public SqlClientTest()
        {
            connection = Substitute.For<IDbConnection>();
            dbCommand = new SqlCommand();
            Func<SqlCommand, Task<int>> nonQueryExecutor = command =>
            {
                Assert.True(
                    command == dbCommand,
                    $"Expected command '{dbCommand}' was '{command}'");

                return Task.FromResult(nonQueryCommandAffectedRows);
            };

            sut = new SqlClient(
                connectionString,
                received =>
                {
                    Assert.True(
                        received == connectionString,
                        $"Expected connection string '{connectionString}' was '{received}'");

                    return connection;
                },
                nonQueryExecutor);
        }

        [Fact]
        public async void ExecuteNonQueryAsyncFlowIsCorrect()
        {
            // Arrange
            var queryText = "query...";
            var parameters = new Dictionary<string, object>
            {
                { "a", "a" },
                { "b", 2 }
            };

            connection.CreateCommand().Returns(dbCommand);

            // Act
            var affectedRows = await sut.ExecuteNonQueryAsync(queryText, parameters);

            // Assert
            connection.Received(1).Open();

            Assert.Equal(queryText, dbCommand.CommandText);
            foreach (var parameter in parameters)
            {
                Assert.Equal(parameter.Value, dbCommand.Parameters[parameter.Key].Value);
            }

            Assert.Equal(nonQueryCommandAffectedRows, affectedRows);
        }


        [Fact]
        public async void ExecuteReaderAsyncFlowIsCorrect()
        {
            // Arrange
            var queryText = "query...";
            var parameters = new Dictionary<string, object>
            {
                { "a", "a" },
                { "b", 2 }
            };

            connection.CreateCommand().Returns(dbCommand);

            var expectedResult = new List<string> { "A", "B" };

            Func<SqlDataReader, List<string>> reader = _ => expectedResult;

            // Act
            var affectedRows = await sut.ExecuteReaderAsync(queryText, parameters, reader);

            // Assert
            connection.Received(1).Open();

            Assert.Equal(queryText, dbCommand.CommandText);
            foreach (var parameter in parameters)
            {
                Assert.Equal(parameter.Value, dbCommand.Parameters[parameter.Key].Value);
            }

            Assert.Equal<List<string>>(expectedResult, affectedRows);
        }
    }
}
