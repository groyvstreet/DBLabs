using Npgsql;

namespace BankSystem.Data
{
    public class DbConnection
    {
        public static NpgsqlCommand getCommand()
        {
            var connectionString = "Server=localhost; Port=5432; Database=banksystemdb; Username=postgres; Password=1kra1ken1;";
            var connection = new NpgsqlConnection(connectionString);
            connection.Open();
            return new(null, connection);
        }
    }
}
