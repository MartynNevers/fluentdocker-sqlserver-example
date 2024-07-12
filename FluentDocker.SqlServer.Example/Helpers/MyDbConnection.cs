namespace FluentDocker.SqlServer.Example.Helpers
{
    using System;
    using System.Data;
    using System.Diagnostics;
    using System.Text.RegularExpressions;
    using Microsoft.Data.SqlClient;

    internal class MyDbConnection : IDisposable
    {
        private readonly string? connectionString;
        private readonly IDbConnection dbConnection;

        public MyDbConnection(string connectionString)
        {
            this.connectionString = connectionString;
            this.dbConnection = new SqlConnection(connectionString);
        }

        public MyDbConnection(SqlConnection sqlConnection)
        {
            if (sqlConnection == null)
            {
                ArgumentNullException.ThrowIfNull(sqlConnection);
            }

            this.dbConnection = sqlConnection;
        }

        public string? ConnectionString
        {
            get
            {
                return this.connectionString;
            }
        }

        public IDbConnection DbConnection
        {
            get
            {
                return this.dbConnection;
            }
        }

        /*
         * Sample Connection String:
         * Data Source="localhost, 14331";User ID=sa;Password=***********;Encrypt=True;Trust Server Certificate=True
         */
        public static string BuildConnectionString(string dataSource, string userId, string password, bool trustServerCertificate = false)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder()
            {
                DataSource = dataSource,
                UserID = userId,
                Password = password,
            };

            if (trustServerCertificate)
            {
                builder.TrustServerCertificate = true;
            }

            return builder.ConnectionString;
        }

        public MyDbConnection Open(TimeSpan timeout)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            while (this.dbConnection.State.Equals(ConnectionState.Closed))
            {
                try
                {
                    this.dbConnection.Open();
                    break;
                }
                catch (SqlException)
                {
                    if (stopwatch.Elapsed > timeout)
                    {
                        throw;
                    }
                    else
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                }
            }

            return this;
        }

        public int ExecuteNonQuery(string command, CommandType commandType, params SqlParameter[] parameters)
        {
            using var sqlCommand = new SqlCommand(command, (SqlConnection)this.dbConnection);
            sqlCommand.CommandType = commandType;
            sqlCommand.Parameters.AddRange(parameters);
            return sqlCommand.ExecuteNonQuery();
        }

        public object ExecuteScalar(string command, CommandType commandType, params SqlParameter[] parameters)
        {
            using var sqlCommand = new SqlCommand(command, (SqlConnection)this.dbConnection);
            sqlCommand.CommandType = commandType;
            sqlCommand.Parameters.AddRange(parameters);
            return sqlCommand.ExecuteScalar();
        }

        public SqlDataReader ExecuteReader(string command, CommandType commandType, params SqlParameter[] parameters)
        {
            using var sqlCommand = new SqlCommand(command, (SqlConnection)this.dbConnection);
            sqlCommand.CommandType = commandType;
            sqlCommand.Parameters.AddRange(parameters);
            return sqlCommand.ExecuteReader();
        }

        public bool ExecuteScript(string scriptPath)
        {
            try
            {
                string script = File.ReadAllText(scriptPath);

                // Split script on GO command
                IEnumerable<string> commands = Regex.Split(
                    script,
                    @"^\s*GO\s*$",
                    RegexOptions.Multiline | RegexOptions.IgnoreCase);

                foreach (string command in commands)
                {
                    if (command.Trim() != string.Empty)
                    {
                        try
                        {
                            this.ExecuteNonQuery(command, CommandType.Text);
                        }
                        catch (SqlException ex)
                        {
                            string formattedCommand = command.Length > 100 ? command.Substring(0, 100) + " ...\n..." : command;
                            Debug.WriteLine($"SQL file error.\nFile: {scriptPath} \nLine: {ex.LineNumber} \nError: {ex.Message} \nSQL Command: \n{formattedCommand}");
                            return false;
                        }
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        public MyDbConnection Close()
        {
            this.dbConnection.Close();
            return this;
        }

        public void Dispose()
        {
            this.dbConnection.Dispose();
        }
    }
}
