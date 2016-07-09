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
        private readonly string connectionString;

        public SqlClient(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public Task<int> ExecuteNonQueryAsync(
            string queryText,
            IDictionary<string, object> parameters = null)
        {
            return ExecuteAsync(queryText, parameters, sqlCmd => sqlCmd.ExecuteNonQueryAsync());
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
            return ExecuteAsync(queryText, parameters, async sqlCmd => readResults(await sqlCmd.ExecuteReaderAsync()));
        }

        private async Task<T> ExecuteAsync<T>(
            string queryText,
            IDictionary<string, object> parameters,
            Func<SqlCommand, Task<T>> readResults)
        {
            parameters = parameters.NullToEmpty();

            using (var sqlConnection = new SqlConnection(connectionString))
            {
                sqlConnection.Open();

                var command = sqlConnection.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = queryText;

                foreach (var parameterPair in parameters)
                {
                    command.Parameters.AddWithValue(parameterPair.Key, parameterPair.Value);
                }

                return await readResults(command);
            }
        }
    }
}
