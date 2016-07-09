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

        // private static readonly Task<SqlDataReader> DefaultReaderExecutor;


        private readonly Func<IDbConnection> createConnection;
        private readonly Func<SqlCommand, Task<int>> nonQueryExecutor;
        // private readonly Task<SqlDataReader> readerExecutor;

        public SqlClient(string connectionString)
            : this(connectionString, cs => new SqlConnection(cs))
        {
        }

        public SqlClient(
            string connectionString,
            Func<string, IDbConnection> connectionCreator = null,
            Func<SqlCommand, Task<int>> nonQueryExecutor = null)
        {
            this.createConnection =
                () => connectionCreator != null ?
                    connectionCreator(connectionString) :
                    DefaultConnectionCreator(connectionString);

            this.nonQueryExecutor = nonQueryExecutor ?? DefaultNonQueryExecutor;

            // this.readerExecutor = readerExecutor ?? DefaultReaderExecutor;
        }

        public Task<int> ExecuteNonQueryAsync(
            string queryText,
            IDictionary<string, object> parameters = null)
        {
            return ExecuteAsync(queryText, parameters, nonQueryExecutor);
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
            return ExecuteAsync(queryText, parameters, async dbCmd => readResults(await dbCmd.ExecuteReaderAsync()));
        }

        private async Task<T> ExecuteAsync<T>(
            string queryText,
            IDictionary<string, object> parameters,
            Func<SqlCommand, Task<T>> readResults)
        {
            parameters = parameters.NullToEmpty();

            using (IDbConnection dbConnection = createConnection())
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
