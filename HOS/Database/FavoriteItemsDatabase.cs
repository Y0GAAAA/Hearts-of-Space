using Global.Util;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading.Tasks;

namespace Client.Database
{
    public class FavoriteItemsDatabase
    {
        #region PATHS
        private static readonly string DATABASE_DIRECTORY = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Hearts of Space"
        );
        private static readonly string DATABASE_FILENAME = Path.Combine(
            DATABASE_DIRECTORY,
            "favorites.db"
        );
        #endregion

        #region SQL STRING
        private const string CREATE_TABLES =
        @"
        CREATE TABLE IF NOT EXISTS albums (
            id INTEGER NOT NULL UNIQUE
        );
        CREATE TABLE IF NOT EXISTS programs (
            id INTEGER NOT NULL UNIQUE
        );
        ";
        private string GetTableName(ItemType type) => type switch
        {
            ItemType.Album => "albums",
            ItemType.Program => "programs",
            _ => throw new InvalidOperationException("unreachable"),
        };
        private string AddSqlString(ItemType type) => $"INSERT INTO {GetTableName(type)}(id) VALUES(@id);";
        private string SelectSqlString(ItemType type) => $"SELECT * FROM {GetTableName(type)};";
        #endregion

        private readonly SqliteConnection connection;

        public enum ItemType
        {
            Album,
            Program,
        }

        public FavoriteItemsDatabase()
        {
            _ = Directory.CreateDirectory(DATABASE_DIRECTORY);

            connection = new SqliteConnection($"Data Source={DATABASE_FILENAME}");
            connection.Open();

            GetCommand(CREATE_TABLES).ExecuteNonQuery();
        }

        private SqliteCommand GetCommand(string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            return command;
        }

        public async Task Add(ItemType type, int id)
        {
            string sql = AddSqlString(type);
            var command = GetCommand(sql);
            command.Parameters.AddWithValue("@id", id);
            try
            {
                await command.ExecuteNonQueryAsync();
            }
            catch (DbException ex) { if (ex.ErrorCode != -2147467259) throw; }
        }
        public async Task<int[]> Get(ItemType type)
        {
            string sql = SelectSqlString(type);
            var reader = await GetCommand(sql).ExecuteReaderAsync();
            var list = new List<int>();
            list.AddWhile(
                () => reader.Read(),
                () => reader.GetInt32(0)
            );
            return list.ToArray();
        }

    }
}
