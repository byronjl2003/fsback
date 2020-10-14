using System;
using MySql.Data.MySqlClient;

namespace fsbackend.Data
{
    public class MySqlDatabase : IDisposable
    {
        public MySqlConnection Connection;

        public MySqlDatabase(string connectionString)
        {
            this.Connection = new MySqlConnection(connectionString);
            this.Connection.Open();
        }

        public void Dispose()
        {
            this.Connection.Close();
        }

    }
}