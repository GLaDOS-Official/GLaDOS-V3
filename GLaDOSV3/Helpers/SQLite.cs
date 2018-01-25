using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;
using System.Data.SQLite;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace GladosV3.Helpers
{
    public static class SqLite
    {
        public static string DirPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Database.db");
        public static SQLiteConnection Connection = new SQLiteConnection($"Data Source={DirPath}");
        public static bool TableExists(this IDbConnection connection, string tableName)
        {
            object result;
            using (SQLiteCommand cmd = new SQLiteCommand(Connection))
            {
                cmd.CommandText = @"SELECT COUNT(*) FROM sqlite_master WHERE name=@TableName";
                var p1 = cmd.CreateParameter();
                p1.DbType = DbType.String;
                p1.ParameterName = "TableName";
                p1.Value = tableName;
                cmd.Parameters.Add(p1);

                result = cmd.ExecuteScalar();
            }

            return ((long)result) == 1;
        }

        public static void CreateTable(this SQLiteConnection connection, string tableName,string parameters)
        {
            string sql = $"CREATE TABLE `{tableName}` ({parameters});";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                command.ExecuteNonQuery();
        }
        public static void SetValue<T>(this SQLiteConnection connection, string tableName, string parameter,T value, string guildid = "")
        {
            string sql = $"UPDATE {tableName} SET {parameter}='{value.ToString()}'";
            if (guildid != "")
                sql += $" WHERE guildid={guildid}";
            using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                command.ExecuteNonQuery();
        }
        public static DataTable GetValues(this SQLiteConnection connection, string tableName,string guildid="")
        {
            string sql = $"SELECT * FROM {tableName}";
            DataTable dt = new DataTable();
            if (guildid != "")
                sql += $" WHERE guildid='{guildid}'";
            using (SQLiteDataAdapter reader = new SQLiteDataAdapter(sql, connection))
                reader.Fill(dt);
            dt.TableName = tableName;
            return dt;
        }
        public static void Start()
        {
            if (!File.Exists(DirPath))
                SQLiteConnection.CreateFile(DirPath);
            Connection.Open();
            if(!Connection.TableExists("servers"))
                Connection.CreateTable("servers", "`guildid` INTEGER, `nsfw` INTEGER, `joinleave_cid` INTEGER, `join_msg` TEXT,  `join_toggle` INTEGER, `leave_msg` TEXT, `leave_toggle` INTEGER");
        }
    }
}
