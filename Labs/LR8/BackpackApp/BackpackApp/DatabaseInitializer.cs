using System;
using System.Data.SqlClient;

namespace BackpackApp
{
    public static class DatabaseInitializer
    {
        private const string MasterConnectionString = @"Data Source=DESKTOP-OV49B0G\SQLEXPRESS;Initial Catalog=master;Integrated Security=True;";
        private const string BackpackConnectionString = @"Data Source=DESKTOP-OV49B0G\SQLEXPRESS;Initial Catalog=backpack;Integrated Security=True;";

        public static void Initialize()
        {
            try
            {
                if (!DatabaseExists())
                {
                    CreateDatabase();
                }

                if (!TableExists())
                {
                    CreateTable();
                    InsertInitialData();
                }
                else
                {
                    if (!TableHasData())
                    {
                        InsertInitialData();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Ошибка инициализации базы данных: " + ex.Message, ex);
            }
        }

        private static bool DatabaseExists()
        {
            string query = "SELECT COUNT(*) FROM sys.databases WHERE name = 'backpack'";
            using (var connection = new SqlConnection(MasterConnectionString))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private static void CreateDatabase()
        {
            string query = "CREATE DATABASE backpack";
            using (var connection = new SqlConnection(MasterConnectionString))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static bool TableExists()
        {
            string query = @"
                SELECT COUNT(*) 
                FROM INFORMATION_SCHEMA.TABLES 
                WHERE TABLE_SCHEMA = 'dbo' AND TABLE_NAME = 'objects'";
            using (var connection = new SqlConnection(BackpackConnectionString))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private static void CreateTable()
        {
            string query = @"
                CREATE TABLE objects (
                    Id INT IDENTITY(1,1) PRIMARY KEY,
                    Name NVARCHAR(100) NOT NULL,
                    Weight INT NOT NULL,
                    Cost INT NOT NULL
                )";
            using (var connection = new SqlConnection(BackpackConnectionString))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }

        private static bool TableHasData()
        {
            string query = "SELECT COUNT(*) FROM objects";
            using (var connection = new SqlConnection(BackpackConnectionString))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                return (int)cmd.ExecuteScalar() > 0;
            }
        }

        private static void InsertInitialData()
        {
            string query = @"
                INSERT INTO objects (Name, Weight, Cost) VALUES
                ('Книга', 1, 600),
                ('Бинокль', 2, 5000),
                ('Аптечка', 4, 1500),
                ('Ноутбук', 2, 40000),
                ('Котелок', 1, 500)";
            using (var connection = new SqlConnection(BackpackConnectionString))
            using (var cmd = new SqlCommand(query, connection))
            {
                connection.Open();
                cmd.ExecuteNonQuery();
            }
        }
    }
}