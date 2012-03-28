using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NORM.Entity;
using NORM.Extensions;

namespace NORM.SQLite
{
    /// <summary>
    /// Класс обертка над Sql Server
    /// </summary>
    public class SQLiteDatabase : Database
    {
        public SQLiteDatabase(string connectionString)
            : base(connectionString)
        {
        }

        public override DbCommand CreateCommand(string cmdText)
        {
            if (string.IsNullOrEmpty(cmdText)) throw new ArgumentNullException("cmdText");

            return new SQLiteCommand(cmdText);
        }

        public override DbParameter CreateParameter(string parameterName, object value)
        {
            if (string.IsNullOrEmpty(parameterName)) throw new ArgumentNullException("parameterName");

            return new SQLiteParameter(parameterName, value);
        }

        public override void CreateDatabase(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            if (IsDatabaseExist(name)) throw new Exception("База данных уже существует");

            var dir = Path.GetDirectoryName(name);
            if (string.IsNullOrEmpty(dir)) throw new Exception("Не удалось создать каталог БД");

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            SQLiteConnection.CreateFile(name);
        }

        public override DataTable GetData(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            DataTable результат;
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    var dataAdapter = new SQLiteDataAdapter(command);
                    var dataSet = new DataSet();
                    dataAdapter.Fill(dataSet);
                    результат = dataSet.Tables[0];
                }
                connection.Close();
            }
            return результат;
        }

        public override DataTable GetData(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            AdjustParameters(ref command);
            DataTable result;
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                var dataAdapter = new SQLiteDataAdapter(command as SQLiteCommand);
                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                result = dataSet.Tables[0];
                connection.Close();
            }
            return result;
        }

        public override void ExecuteQuery(string query)
        {
            if (string.IsNullOrEmpty(query)) throw new ArgumentNullException("query");

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SQLiteCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public override int ExecuteInsert(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            AdjustParameters(ref command);
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = "SELECT last_insert_rowid() AS ID";
                int id = command.ExecuteScalar().ToInt();

                connection.Close();
                return id;
            }
        }

        public override void ExecuteCommand(DbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            AdjustParameters(ref command);
            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        public override string CreateColumnQuery(PropertyInfo property, ColumnAttribute column)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (column == null) throw new ArgumentNullException("column");

            var type = column.Type ?? property.PropertyType;
            var name = property.Name;
            var result = new StringBuilder(string.Format(" {0} ", name));

            // Тип данных

            if (type == typeof(DateTime)) result.Append("TEXT");
            else if (type == typeof(DateTime?)) result.Append("TEXT");
            else if (type == typeof(int)) result.Append("INTEGER");
            else if (type == typeof(int?)) result.Append("INTEGER");
            else if (type == typeof(long)) result.Append("INTEGER");
            else if (type == typeof(long?)) result.Append("INTEGER");
            else if (type == typeof(float)) result.Append("REAL");
            else if (type == typeof(float?)) result.Append("REAL");
            else if (type == typeof(decimal)) result.Append("REAL");
            else if (type == typeof(decimal?)) result.Append("REAL");
            else if (type == typeof(string)) result.Append("TEXT");
            else if (type == typeof(bool)) result.Append("INTEGER");
            else if (type == typeof(bool?)) result.Append("INTEGER");
            else if (type.IsEnum) result.Append("INTEGER");
            else throw new Exception("Неизвестный тип данных");

            // Ограничения

            if (column.Primary) result.Append(" PRIMARY KEY");
            if (column.AutoIncrement) result.Append(" AUTOINCREMENT");
            if (column.Unique) result.Append(" UNIQUE");
            if (column.NotNull) result.Append(" NOT NULL");
            if (!string.IsNullOrEmpty(column.Default))
                result.AppendFormat(" DEFAULT {0}", column.Default);

            // Результат

            return result.ToString();
        }

        public override IEnumerable<string> GetTables(string database = null)
        {
            const string query = @"SELECT name FROM SQLITE_MASTER where type='table'";
            return GetData(query).Rows.Cast<DataRow>().Select(
                row => string.Format("{0}", row["name"]));
        }

        public override IEnumerable<string> GetDatabases()
        {
            var sqLiteConnectionStringBuilder = new SQLiteConnectionStringBuilder(ConnectionString);
            if (File.Exists(sqLiteConnectionStringBuilder.DataSource))
            {
                return new[] { sqLiteConnectionStringBuilder.DataSource };
            }
            else
            {
                return new string[0];
            }
        }

        public override void DropDatabase(string name)
        {
            var sqLiteConnectionStringBuilder = new SQLiteConnectionStringBuilder(ConnectionString);
            File.Delete(sqLiteConnectionStringBuilder.DataSource);
        }

        #region NotImplementedException

        public override DataTable SelectAll(string database, string table)
        {
            var query = string.Format(@"SELECT * FROM {0}", table);
            return GetData(query);
        }

        public override string CreateIdentityInsertQuery(string tableName, bool enabled)
        {
            throw new NotImplementedException();
        }

        #endregion NotImplementedException
    }
}
