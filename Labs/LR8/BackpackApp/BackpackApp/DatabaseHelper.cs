using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using BackpackApp.Models;
using BackpackApp.Debugging;

namespace BackpackApp
{
    public static class DatabaseHelper
    {
        private const string ConnectionString = @"Data Source=DESKTOP-OV49B0G\SQLEXPRESS;Initial Catalog=backpack;Integrated Security=True;";

        public static List<Item> GetAllItems()
        {
            var items = new List<Item>();
            string query = "SELECT Id, Name, Weight, Cost FROM objects";

            DebugLogger.LogSqlQuery(query);

            using (var connection = new SqlConnection(ConnectionString))
            {
                using (var cmd = new SqlCommand(query, connection))
                {
                    try
                    {
                        connection.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new Item
                                {
                                    Id = reader.GetInt32(0),
                                    Name = reader.GetString(1),
                                    Weight = reader.GetInt32(2),
                                    Cost = reader.GetInt32(3)
                                });
                            }
                        }
                        DebugLogger.Log($"Загружено {items.Count} предметов из БД");
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Log($"Ошибка при загрузке данных: {ex.Message}");
                        throw;
                    }
                }
            }

            return items;
        }
    }
}