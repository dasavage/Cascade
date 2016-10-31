using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;

namespace Cascade.Core.Database
{
    public class DatabaseManager
    {
        private readonly string _connectionString;

        public DatabaseManager(DatabaseSettings databaseSettings)
        {
            var mysqlConnectionString = new MySqlConnectionStringBuilder
            {
                ConnectionLifeTime = 300,
                ConnectionTimeout = 30,
                Database = databaseSettings.DatabaseName,
                DefaultCommandTimeout = 120,
                Logging = false,
                MaximumPoolSize = (uint)databaseSettings.MaximumConnections,
                MinimumPoolSize = 3,
                Password = databaseSettings.DatabasePassword,
                Pooling = true,
                Port = (uint)databaseSettings.DatabasePort,
                Server = databaseSettings.DatabaseHost,
                UseCompression = false,
                UserID = databaseSettings.DatabaseUsername
            };

            _connectionString = mysqlConnectionString.ToString();
        }

        public bool WorkingConnection()
        {
            try
            {
                using (var databaseConnection = GetConnection())
                {
                    databaseConnection.OpenConnection();
                    databaseConnection.CloseConnection();
                }
            }
            catch (MySqlException)
            {
                return false;
            }

            return true;
        }

        public DatabaseConnection GetConnection()
        {
            return new DatabaseConnection(_connectionString);
        }
    }
}
