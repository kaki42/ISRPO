using System;
using System.Data.SqlClient;

namespace FilesApp
{
    /// <summary>
    /// Вспомогательный класс для автоматического создания базы данных и таблицы FileOperations.
    /// </summary>
    public static class DatabaseHelper
    {
        /// <summary>
        /// Проверяет существование базы данных и таблицы FileOperations.
        /// Если база отсутствует – создаёт её. Если таблица отсутствует – создаёт таблицу.
        /// </summary>
        /// <param name="connectionString">Строка подключения к целевой базе данных (например, "Data Source=...;Initial Catalog=...;Integrated Security=True;")</param>
        public static void EnsureDatabaseAndTable(string connectionString)
        {
            try
            {
                // Извлекаем имя сервера и имя базы из строки подключения
                var builder = new SqlConnectionStringBuilder(connectionString);
                string server = builder.DataSource;
                string databaseName = builder.InitialCatalog;

                // Строка подключения к системной базе master
                string masterConnectionString = $"Data Source={server};Initial Catalog=master;Integrated Security=True;";

                // 1. Подключаемся к master и проверяем наличие базы данных
                using (var masterConnection = new SqlConnection(masterConnectionString))
                {
                    masterConnection.Open();
                    string checkDbQuery = "SELECT database_id FROM sys.databases WHERE Name = @dbName";
                    using (var cmd = new SqlCommand(checkDbQuery, masterConnection))
                    {
                        cmd.Parameters.AddWithValue("@dbName", databaseName);
                        var dbId = cmd.ExecuteScalar();
                        if (dbId == null) // базы нет – создаём
                        {
                            string createDbQuery = $"CREATE DATABASE {databaseName}";
                            using (var createCmd = new SqlCommand(createDbQuery, masterConnection))
                            {
                                createCmd.ExecuteNonQuery();
                            }
                        }
                    }
                }

                // 2. Теперь подключаемся к целевой базе и создаём таблицу FileOperations, если её нет
                using (var connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string checkTableQuery = @"
                        IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FileOperations' AND xtype='U')
                        BEGIN
                            CREATE TABLE FileOperations (
                                Id INT PRIMARY KEY IDENTITY(1,1),
                                FilePath NVARCHAR(500),
                                Content NVARCHAR(MAX),
                                SymbolCount INT,
                                OperationType NVARCHAR(50),
                                OperationDate DATETIME DEFAULT GETDATE()
                            )
                        END";
                    using (var cmd = new SqlCommand(checkTableQuery, connection))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                // Пробрасываем исключение выше, чтобы форма могла его обработать
                throw new Exception("Ошибка при инициализации базы данных: " + ex.Message, ex);
            }
        }
    }
}