using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using NORM35.Entity;

namespace NORM35
{
    /// <summary>
    /// Базовый класс для различных реализаций БД
    /// </summary>
    public abstract class Database
    {
        public string ConnectionString { get; protected set; }

        protected Database(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString)) throw new ArgumentNullException("connectionString");

            ConnectionString = connectionString;
        }

        protected void AdjustParameters(ref DbCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");

            foreach (DbParameter parameter in command.Parameters)
            {
                if (parameter.Value == null)
                {
                    parameter.Value = DBNull.Value;
                }
            }
        }

        public abstract DbCommand CreateCommand(string cmdText);

        public abstract DbParameter CreateParameter(string parameterName, object value);

        public abstract string CreateColumnQuery(PropertyInfo property, ColumnAttribute column);

        public abstract string CreateIdentityInsertQuery(string tableName, bool enabled);

        public abstract IEnumerable<string> GetDatabases();

        public bool IsDatabaseExist(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            return GetDatabases().Contains(name);
        }

        public abstract void CreateDatabase(string name);

        public abstract void DropDatabase(string name);

        public abstract IEnumerable<string> GetTables(string database = null);

        public bool IsTableExist(string name, string database = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");

            return GetTables(database).Contains(name);
        }

        public abstract DataTable SelectAll(string database, string table);

        public abstract DataTable GetData(string query);

        public abstract DataTable GetData(DbCommand command);

        public abstract void ExecuteQuery(string query);

        public abstract int ExecuteInsert(DbCommand command);

        public abstract void ExecuteCommand(DbCommand command);
    }
}
