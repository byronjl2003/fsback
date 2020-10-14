using System;
using fsbackend.Data;
using fsbackend.Models;
using MySql.Data.MySqlClient;

namespace fsbackend.services
{
    public class AuthService : IDisposable
    {
        public MySqlDatabase db;

        public AuthService(MySqlDatabase mySqlDatabase)
        {
            this.db = mySqlDatabase;
        }

        public void Dispose()
        {
            Console.WriteLine("Disposable de AuthService");
        }

        public async System.Threading.Tasks.Task<User> LoginAsync(string user, string pass)
        {
            User t = null;
            var cmd = this.db.Connection.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT email,name,pass FROM user WHERE email = @user";
            cmd.Parameters.Add(new MySqlParameter
            {
                ParameterName = "@user",
                DbType = System.Data.DbType.String,
                Value = user,
            });
            using (var reader = await cmd.ExecuteReaderAsync())
                while (await reader.ReadAsync())
                {
                    t = new User();
                    {
                        t.Email = reader.GetFieldValue<string>(0);
                        t.Nombre = reader.GetFieldValue<string>(1);
                        t.Pass = reader.GetFieldValue<string>(2);

                    };
                }

            if (t.Pass == pass)
            {
                return t;
            }
            else
            {
                return null;
            }
        }
    }
}