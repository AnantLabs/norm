using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using NORM.Entity;
using NORM.Extensions;

namespace NORM.SqlServer
{
    /// <summary>
    /// Класс обертка над Sql Server
    /// </summary>
    public class SqlServerDatabase : Database
    {
        public SqlServerDatabase(string connectionString)
            : base(connectionString)
        {
        }

        public override DbCommand CreateCommand(string cmdText)
        {
            return new SqlCommand(cmdText);
        }

        public override DbParameter CreateParameter(string parameterName, object value)
        {
            return new SqlParameter(parameterName, value);
        }

        public string GetMachineName()
        {
            var data = GetData(@"SELECT SERVERPROPERTY('MachineName')");
            try
            {
                return data.Rows[0][0].ToString();
            }
            catch
            {
                return Environment.MachineName;
            }
        }

        public bool CheckTriggerExist(string triggerName)
        {
            string query = string.Format("SELECT * FROM sys.triggers WHERE name = @triggerName");
            DataTable data;
            using (var command = CreateCommand(query))
            {
                command.Parameters.Add(CreateParameter("@triggerName", triggerName));
                data = GetData(command);
            }

            return data.Rows.Count > 0;
        }

        public override IEnumerable<string> GetDatabases()
        {
            const string query = @"SELECT name FROM sys.databases";
            var data = GetData(query);
            return data.Rows.Cast<DataRow>().Select(row => row["name"].ToString());
        }

        public override void CreateDatabase(string name)
        {
            var query = string.Format(@"CREATE DATABASE {0}", name);
            ExecuteQuery(query);
        }

        public override void DropDatabase(string name)
        {
            var query = string.Format(@"DROP DATABASE {0}", name);
            ExecuteQuery(query);
        }

        public override DataTable SelectAll(string database, string table)
        {
            var query = string.Format(@"SELECT * FROM {0}.{1}", database, table);
            return GetData(query);
        }

        public override IEnumerable<string> GetTables(string database = null)
        {
            string query;
            if (!string.IsNullOrEmpty(database))
            {
                query = string.Format(
@"SELECT s.name + '.' + t.name as name FROM {0}.sys.tables t
JOIN {0}.sys.schemas s ON s.schema_id = t.schema_id", database);
            }
            else
            {
                query = 
@"SELECT s.name + '.' + t.name as name FROM sys.tables t
JOIN sys.schemas s ON s.schema_id = t.schema_id";
            }

            return GetData(query).Rows.Cast<DataRow>().Select(
                row => string.Format("{0}", row["name"]));
        }

        public override DataTable GetData(string query)
        {
            DataTable result;
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    var dataAdapter = new SqlDataAdapter(command);
                    var dataSet = new DataSet();
                    dataAdapter.Fill(dataSet);
                    result = dataSet.Tables[0];
                }
                connection.Close();
            }
            return result;
        }

        public override DataTable GetData(DbCommand command)
        {
            AdjustParameters(ref command);
            DataTable result;
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                var dataAdapter = new SqlDataAdapter(command as SqlCommand);
                var dataSet = new DataSet();
                dataAdapter.Fill(dataSet);
                result = dataSet.Tables[0];
                connection.Close();
            }
            return result;
        }

        public override void ExecuteQuery(string query)
        {
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                using (var command = new SqlCommand(query, connection))
                {
                    command.CommandType = CommandType.Text;
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }

        public override int ExecuteInsert(DbCommand command)
        {
            AdjustParameters(ref command);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();

                command.Parameters.Clear();
                command.CommandText = "SELECT @@IDENTITY";
                int id = command.ExecuteScalar().ToInt();

                connection.Close();
                return id;
            }
        }

        public override void ExecuteCommand(DbCommand command)
        {
            AdjustParameters(ref command);
            using (var connection = new SqlConnection(ConnectionString))
            {
                connection.Open();
                command.Connection = connection;
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                connection.Close();
            }
        }

        protected void EnableClr()
        {
            var query = string.Format(
@"EXEC sp_configure 'show advanced options' , '1';
reconfigure;
EXEC sp_configure 'clr enabled' , '1'
reconfigure;
-- Turn advanced options back off
EXEC sp_configure 'show advanced options' , '0';");
            ExecuteQuery(query);

//            query = string.Format(
//@"ALTER DATABASE {0} SET TRUSTWORTHY ON;", Settings.DatabaseName);
//            ExecuteQuery(query);
        }

        public override string CreateColumnQuery(PropertyInfo property, ColumnAttribute column)
        {
            var type = column.Type ?? property.PropertyType;
            var name = property.Name;
            var result = new StringBuilder(string.Format(" {0} ", name));

            // Тип данных

            if (type == typeof(XmlDocument)) result.Append("xml");
            else if (type == typeof(object)) result.Append("sql_variant");
            else if (type == typeof(DateTime)) result.Append("datetime2");
            else if (type == typeof(DateTime?)) result.Append("datetime2");
            else if (type == typeof(Guid)) result.Append("uniqueidentifier");
            else if (type == typeof(int)) result.Append("int");
            else if (type == typeof(float)) result.Append("float");
            else if (type == typeof(decimal)) result.Append("real");
            else if (type == typeof(long)) result.Append("bigint");
            else if (type == typeof(string)) result.AppendFormat("nvarchar({0})", column.Size);
            else if (type == typeof(bool)) result.Append("bit");
            else if (type.IsEnum) result.Append("int");
            else throw new Exception("Неизвестный тип данных");

            // Ограничения

            if (column.Primary) result.Append(" PRIMARY KEY");
            if (column.AutoIncrement) result.Append(" IDENTITY");
            if (column.Unique) result.Append(" UNIQUE");
            if (column.NotNull) result.Append(" NOT NULL");
            if (!string.IsNullOrEmpty(column.Default))
                result.AppendFormat(" DEFAULT {0}", column.Default);

            // Результат

            return result.ToString();
        }

        public override string CreateIdentityInsertQuery(string tableName, bool enabled)
        {
            return string.Format("SET IDENTITY_INSERT {0} {1}", tableName, enabled ? "ON" : "OFF");
        }
    }
}
