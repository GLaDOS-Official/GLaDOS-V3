using System.Data;
using System.Data.SQLite;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace GLaDOSV3.Helpers
{
    [SuppressMessage("ReSharper", "ArrangeMethodOrOperatorBody")]
    [SuppressMessage("Style", "IDE0022:Use expression body for methods", Justification = "<Pending>")]
    public static class SqLite
    {
        private static readonly string DirPath = Path.Combine(Directory.GetCurrentDirectory(), "Database.db");

        //use shared cache so it will be faster (SPEEEEEEED!!)
        public static SQLiteConnection Connection = new SQLiteConnection($"Data Source={DirPath};Cache=Shared;Version=3;Pooling=True;Max Pool Size=100");


        /// <summary>
        ///  Returns a bool if a table exists.
        /// </summary>
        public static Task<bool> TableExistsAsync(this SQLiteConnection connection, string tableName, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                object result;
                using (SQLiteCommand cmd = new SQLiteCommand(connection))
                {
                    cmd.CommandText = @"SELECT COUNT(*) FROM sqlite_master WHERE name=@TableName";
                    var p1 = cmd.CreateParameter();
                    p1.DbType = DbType.String;
                    p1.ParameterName = "TableName";
                    p1.Value = tableName;
                    cmd.Parameters.Add(p1);
                    result = cmd.ExecuteScalar();
                }

                return Task.FromResult(((long)result) >= 1);
            }, token);
        }
        /// <summary>
        /// Creates a table with a name and parameters..
        /// </summary>
        public static Task CreateTableAsync(this SQLiteConnection connection, string tableName, string parameters, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                var sql = $"CREATE TABLE `{tableName}` ({parameters});";
                if (!ValidateSQLSafety(sql)) return Task.CompletedTask;
                using SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                return Task.CompletedTask;
            }, token);
        }
        /// <summary>
        /// Sets/updates a value in a table.
        /// </summary>
        public static Task SetValueAsync<T>(this SQLiteConnection connection, string tableName, string parameter, T value, string filter = "", CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                var sql = $"UPDATE {tableName} SET {parameter}='{value}'";
                if (!string.IsNullOrEmpty(filter))
                    sql += $" {filter}";
                if (!ValidateSQLSafety(sql)) return Task.CompletedTask;
                using SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                return Task.CompletedTask;
            }, token);
        }
        /// <summary>
        /// Returns a DataTable.
        /// </summary>
        public static Task<DataTable> GetValuesAsync(this SQLiteConnection connection, string tableName, string filter = "", CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                var sql = $"SELECT * FROM {tableName}";
                using DataTable dt = new DataTable();
                if (!string.IsNullOrEmpty(filter))
                    sql += $" {filter}";
                if (!ValidateSQLSafety(sql)) return Task.FromResult(new DataTable());
                using (SQLiteDataAdapter reader = new SQLiteDataAdapter(sql, connection))
                    reader.Fill(dt);
                dt.TableName = tableName;
                return Task.FromResult(dt);
            }, token);
        }
        /// <summary>
        /// Starts SQLite connection and checks for tables.
        /// </summary>
        public static void Start()
        {
            if (!File.Exists(DirPath))
                SQLiteConnection.CreateFile(DirPath);
            if ((File.GetAttributes(DirPath) & FileAttributes.ReadOnly) != 0)
                File.SetAttributes(DirPath, File.GetAttributes(DirPath) & ~FileAttributes.ReadOnly);
            Connection.Open();

            //Enable write-ahead logging
            var walCommand = Connection.CreateCommand();
            walCommand.CommandText = @"PRAGMA auto_vacuum = FULL;PRAGMA journal_mode = 'wal';PRAGMA synchronous=OFF;";
            walCommand.ExecuteNonQuery();
            //CREATE TABLE "servers" ( `guildid` INTEGER, `nsfw` INTEGER, `joinleave_cid` INTEGER, `join_msg` TEXT, `join_toggle` INTEGER, `leave_msg` TEXT, `leave_toggle` INTEGER, `prefix` TEXT )
            if (!Connection.TableExistsAsync("servers").GetAwaiter().GetResult())
                Connection.CreateTableAsync("servers", "`guildid` INTEGER, `nsfw` INTEGER, `joinleave_cid` INTEGER, `join_msg` TEXT, `join_toggle` INTEGER, `leave_msg` TEXT, `leave_toggle` INTEGER, `prefix` TEXT");
            if (!Connection.TableExistsAsync("BlacklistedUsers").GetAwaiter().GetResult())
                Connection.CreateTableAsync("BlacklistedUsers", "`UserId` INTEGER,`Reason` TEXT DEFAULT \'Unspecified.\', `Date` INTEGER");
            if (!Connection.TableExistsAsync("BotSettings").GetAwaiter().GetResult())
                Connection.CreateTableAsync("BotSettings", "`ID` INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT, `name` TEXT, `value` TEXT");
            if (!Connection.TableExistsAsync("BlacklistedServers").GetAwaiter().GetResult())
                Connection.CreateTableAsync("BlacklistedServers", "`guildid` INTEGER, `date` TEXT, `reason` TEXT");
        }

        /// <summary>
        /// Adds a new row into a table.
        /// </summary>
        public static Task AddRecordAsync<T>(this SQLiteConnection connection, string tablename, string values, T[] items, string filter = "", CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(tablename) || string.IsNullOrWhiteSpace(values) || items == null)
                    return Task.CompletedTask;
                var result = string.Empty;
                for (var i = 1; i <= items?.Length; i++) { result += $"@val{i},"; }

                var sql = $"INSERT INTO {tablename} ({values}) VALUES ({result.Remove(result.Length - 1)}) ";
                if (!string.IsNullOrEmpty(filter))
                    sql += $"WHERE {filter}";
                if (!ValidateSQLSafety(sql)) return Task.FromResult(false);
                using SQLiteCommand command = new SQLiteCommand(sql, connection);
                for (var i = 1; i <= items.Length; i++)
                    command.Parameters.AddWithValue($"@val{i}", items[i - 1]);
                command.ExecuteNonQuery();
                return Task.CompletedTask;
            }, token);
        }
        /// <summary>
        /// Removes a row from the selected table.
        /// </summary>
        public static Task RemoveRecordAsync(this SQLiteConnection connection, string tablename, string filter, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(filter))
                    return Task.FromException(new SQLiteException("Filter mustn't be empty!"));
                var sql = $"DELETE FROM {tablename} WHERE {filter}";
                if (!ValidateSQLSafety(sql)) return Task.FromResult(false);
                using SQLiteCommand command = new SQLiteCommand(sql, connection);
                command.ExecuteNonQuery();
                return Task.CompletedTask;
            }, token);
        }
        /// <summary>
        /// Returns true if table exists
        /// </summary>
        public static Task<bool> RecordExistsAsync(this SQLiteConnection connection, string tablename, string filter = "", CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                var sql = $"SELECT 1 FROM {tablename}";
                if (!string.IsNullOrEmpty(filter)) sql += $" {filter}";
                if (!ValidateSQLSafety(sql)) return Task.FromResult(false);
                using DataTable dt = new DataTable();
                using (SQLiteDataAdapter reader = new SQLiteDataAdapter(sql, connection))
                    reader.Fill(dt);
                dt.TableName = tablename;
                return Task.FromResult(dt.Rows.Count != 0);
            }, token);
        }
        /// <summary>
        ///  Executes SQL command
        /// </summary>
        public static Task ExecuteSqlAsync(this SQLiteConnection connection, string command, CancellationToken token = default)
        {
            return Task.Run(() =>
            {
                if (string.IsNullOrWhiteSpace(command) && !ValidateSQLSafety(command))
                    return Task.FromException(new SQLiteException("Command mustn't be empty!!"));
                using SQLiteCommand sqlCommand = new SQLiteCommand(command, connection);
                sqlCommand.ExecuteNonQuery();
                return Task.CompletedTask;
            }, token);
        }

        private static bool ValidateSQLSafety(string command)
        {
            //Safety features
            Connection.Flags |= SQLiteConnectionFlags.NoLoadExtension;
            if (command.Contains("--") || (command.Contains("/*") && command.Contains("*/"))) return false; // comments are used for SQLi, since SQL is mostly used for programmers, we strictly disallow them
            if (command.Contains("INTO OUTFILE")) return false; // prevent file write

            return true;
        }
    }
}
