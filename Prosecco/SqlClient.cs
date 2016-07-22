using Peppermint.Dictionaries;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Prosecco
{
    public class SqlClient
    {
        private static readonly Func<string, IDbConnection> DefaultConnectionCreator =
            connectionString => new SqlConnection(connectionString);

        private static readonly Func<SqlCommand, Task<int>> DefaultNonQueryExecutor =
            dbCmd => dbCmd.ExecuteNonQueryAsync();

        private static readonly Func<SqlCommand, Task<SqlDataReader>> DefaultReaderExecutor =
            dbCmd => dbCmd.ExecuteReaderAsync();

        private static readonly Func<SqlCommand, Task<object>> DefaultScalarExecutor =
            dbCmd => dbCmd.ExecuteScalarAsync();


        private readonly Func<IDbConnection> createConnection;
        private readonly Func<SqlCommand, Task<int>> executeNonQuery;
        private readonly Func<SqlCommand, Task<SqlDataReader>> executeReader;
        private readonly Func<SqlCommand, Task<object>> scalarExecutor;

        public SqlClient(string connectionString)
            : this(connectionString, runtimeConnectionString => new SqlConnection(runtimeConnectionString))
        {
        }

        public SqlClient(
            string connectionString,
            Func<string, IDbConnection> connectionCreator = null,
            Func<SqlCommand, Task<int>> executeNonQuery = null,
            Func<SqlCommand, Task<SqlDataReader>> executeReader = null,
            Func<SqlCommand, Task<object>> scalarExecutor = null)
        {
            this.createConnection =
                () => connectionCreator != null ?
                    connectionCreator(connectionString) :
                    DefaultConnectionCreator(connectionString);

            this.executeNonQuery = executeNonQuery ?? DefaultNonQueryExecutor;

            this.executeReader = executeReader ?? DefaultReaderExecutor;

            this.scalarExecutor = scalarExecutor ?? DefaultScalarExecutor;
        }

        public Task<int> ExecuteNonQueryAsync(
            string queryText,
            IDictionary<string, object> parameters = null)
        {
            return ExecuteAsync(queryText, parameters, executeNonQuery);
        }
        
        public Task<T> ExecuteReaderAsync<T>(
            string queryText,
            Func<SqlDataReader, T> run)
        {
            return ExecuteReaderAsync(queryText, null, run);
        }

        public Task<T> ExecuteReaderAsync<T>(
            string queryText,
            IDictionary<string, object> parameters,
            Func<SqlDataReader, T> readResults)
        {
            return ExecuteAsync(queryText, parameters, async dbCmd => readResults(await executeReader(dbCmd)));
        }

        public Task<object> ExecuteScalarAsync(
            string queryText,
            IDictionary<string, object> parameters)
        {
            return ExecuteAsync(queryText, parameters, async dbCmd => await scalarExecutor(dbCmd));
        }

        private async Task<T> ExecuteAsync<T>(
            string queryText,
            IDictionary<string, object> parameters,
            Func<SqlCommand, Task<T>> readResults)
        {
            parameters = parameters.NullToEmpty();

            using (var dbConnection = createConnection())
            {
                dbConnection.Open();

                var command = (SqlCommand)dbConnection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = queryText;

                foreach (var parameterPair in parameters)
                {
                    var parameter = command.CreateParameter();
                    parameter.ParameterName = parameterPair.Key;
                    parameter.Value = parameterPair.Value;

                    command.Parameters.Add(parameter);
                }

                return await readResults(command);
            }
        }
    }
}
