﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using ConstructionSet;
using NSubstitute;
using Xunit;

namespace Prosecco.Tests
{
    public class SqlClientTest
    {
        private const string ConnectionString = @"FAKE";
        private const string QueryText = @"SELECT FROM abc;";
        private readonly Dictionary<string, object> QueryParameters =
            new Dictionary<string, object>
            {
                { "a", "a" },
                { "b", 2 }
            };

        private readonly SqlClient sut;

        private readonly IDbConnection connection;
        private readonly SqlCommand dbCommand;
        const int nonQueryCommandAffectedRows = 1;
        const string scalarResult = "scalar result";
        private SqlDataReader dataReader;
        private XmlReader xmlReader;

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
            Func<SqlCommand, Task<SqlDataReader>> readerExecutor = command =>
            {
                Assert.True(
                    command == dbCommand,
                    $"Expected command '{dbCommand}' was '{command}'");

                dataReader = Create<SqlDataReader>.UsingPrivateConstructor(dbCommand, CommandBehavior.Default);
                return Task.FromResult(dataReader);
            };
            Func<SqlCommand, Task<object>> scalarExecutor = command =>
            {
                Assert.True(
                    command == dbCommand,
                    $"Expected command '{dbCommand}' was '{command}'");

                return Task.FromResult((object)scalarResult);
            };
            Func<SqlCommand, Task<XmlReader>> xmlReaderExecutor = command =>
            {
                Assert.True(
                    command == dbCommand,
                    $"Expected command '{dbCommand}' was '{command}'");

                xmlReader = Substitute.For<XmlReader>();
                return Task.FromResult(xmlReader);
            };

            sut = new SqlClient(
                ConnectionString,
                received =>
                {
                    Assert.True(
                        received == ConnectionString,
                        $"Expected connection string '{ConnectionString}' was '{received}'");

                    return connection;
                },
                nonQueryExecutor,
                readerExecutor,
                scalarExecutor,
                xmlReaderExecutor);
        }

        [Fact]
        public async void ExecuteNonQueryAsyncFlowIsCorrect()
        {
            // Arrange
            connection.CreateCommand().Returns(dbCommand);

            // Act
            var affectedRows = await sut.ExecuteNonQueryAsync(QueryText, QueryParameters);

            // Assert
            connection.Received(1).Open();

            Assert.Equal(QueryText, dbCommand.CommandText);
            Assert.True(ParametersRelayedCorrectly());

            Assert.Equal(nonQueryCommandAffectedRows, affectedRows);
        }


        [Fact]
        public async void ExecuteReaderAsyncFlowIsCorrect()
        {
            // Arrange
            connection.CreateCommand().Returns(dbCommand);

            var expectedResult = new List<string> { "A", "B" };

            Func<SqlDataReader, List<string>> reader = receivedDataReader =>
            {
                Assert.Same(dataReader, receivedDataReader);
                return expectedResult;
            };

            // Act
            var affectedRows = await sut.ExecuteReaderAsync(QueryText, QueryParameters, reader);

            // Assert
            connection.Received(1).Open();

            Assert.Equal(QueryText, dbCommand.CommandText);
            Assert.True(ParametersRelayedCorrectly());

            Assert.Equal<List<string>>(expectedResult, affectedRows);
        }

        [Fact]
        public async void ExecuteScalarAsyncFlowIsCorrect()
        {
            // Arrange
            connection.CreateCommand().Returns(dbCommand);

            // Act
            var result = await sut.ExecuteScalarAsync(QueryText, QueryParameters);

            // Assert
            connection.Received(1).Open();

            Assert.Equal(QueryText, dbCommand.CommandText);
            Assert.True(ParametersRelayedCorrectly());

            Assert.Equal(scalarResult, result as string);
        }

        [Fact]
        public async void ExecuteXmlReaderAsyncFlowIsCorrect()
        {
            // Arrange
            connection.CreateCommand().Returns(dbCommand);

            var expectedResult = new List<string> { "A", "B" };

            Func<XmlReader, List<string>> reader = receivedDataReader =>
            {
                Assert.Same(xmlReader, receivedDataReader);
                return expectedResult;
            };

            // Act
            var result = await sut.ExecuteXmlReaderAsync(QueryText, QueryParameters, reader);

            // Assert
            connection.Received(1).Open();

            Assert.Equal(QueryText, dbCommand.CommandText);
            Assert.True(ParametersRelayedCorrectly());

            Assert.Equal<List<string>>(expectedResult, result);
        }

        private bool ParametersRelayedCorrectly()
        {
            var parameterCountEquals =
                QueryParameters.Count == dbCommand.Parameters.Count;
            var parametersRelayedCorrectly =
                QueryParameters.All(p => dbCommand.Parameters[p.Key].Value == p.Value);

            return parameterCountEquals && parametersRelayedCorrectly;
        }
    }
}
