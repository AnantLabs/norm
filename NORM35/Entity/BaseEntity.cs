using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using NORM35.Extensions;
using NORM35.Reflection;

namespace NORM35.Entity
{
    /// <summary>
    /// Базовый класс для сущностей БД
    /// </summary>
    /// <typeparam name="T">Сущность БД</typeparam>
    /// <typeparam name="TDatabase">Класс для работы с БД</typeparam>
    public abstract class BaseEntity<T, TDatabase>
        where T : BaseEntity<T, TDatabase>, new()
        where TDatabase: Database, new()
    {
        static BaseEntity()
        {
            _database = new TDatabase();
        }

        protected static Database _database;

        [Column(Primary = true, AutoIncrement = true, Order = 1)]
        public virtual int Id { get; set; }

        protected static string _tableName;

        protected static string _databaseName;

        protected static IEnumerable<PropertyInfo> _columns;

        public static string TableName
        {
            get
            {
                if (string.IsNullOrEmpty(_tableName))
                {
                    var attribute = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
                    _tableName = attribute.Name;
                }

                return _tableName;
            }
        }

        public static string DatabaseName
        {
            get
            {
                if (string.IsNullOrEmpty(_databaseName))
                {
                    var attribute = (TableAttribute)typeof(T).GetCustomAttributes(typeof(TableAttribute), false).First();
                    _databaseName = attribute.Database;
                }

                return _databaseName;
            }
        }

        public static string FullTableName
        {
            get
            {
                if (!string.IsNullOrEmpty(DatabaseName))
                {
                    return string.Format("{0}.{1}", DatabaseName, TableName);
                }
                else
                {
                    return TableName;
                }
            }
        }

        public static IEnumerable<PropertyInfo> Columns
        {
            get
            {
                if (_columns == null)
                {
                    _columns = typeof(T).
                        GetProperties().
                        Where(x => x.GetCustomAttributes(typeof(ColumnAttribute), true).Length > 0);
                }

                return _columns;
            }
        }

        public virtual void Save()
        {
            if (Id > 0)
            {
                Update();
            }
            else
            {
                Insert();
            }
        }

        protected virtual void Insert()
        {
            var stringBuilder = new StringBuilder("INSERT INTO");
            stringBuilder.AppendFormat(" {0} ", FullTableName);

            var stringFields = new StringBuilder();
            var stringValues = new StringBuilder();
            foreach (var property in Columns)
            {
                if (property.Name.ToUpper() == "ID") continue;
                stringFields.AppendFormat("{0}, ", property.Name);
                stringValues.AppendFormat("@{0}, ", property.Name);
            }
            stringFields.Remove(stringFields.Length - 2, 2);
            stringValues.Remove(stringValues.Length - 2, 2);
            stringBuilder.AppendFormat("({0}) ", stringFields);
            stringBuilder.AppendFormat("VALUES ({0})", stringValues);

            var database = new TDatabase();
            using (var command = database.CreateCommand(stringBuilder.ToString()))
            {
                foreach (var property in Columns)
                {
                    if (property.Name.ToUpper() == "ID") continue;
                    var paramName = string.Format("@{0}", property.Name);
                    command.Parameters.Add(database.CreateParameter(paramName, property.GetValue(this, null)));
                }

                Id = _database.ExecuteInsert(command);
            }
        }

        protected virtual void Update()
        {
            var stringBuilder = new StringBuilder("UPDATE");
            stringBuilder.AppendFormat(" {0} SET ", FullTableName);
            foreach (var property in Columns)
            {
                if (property.Name.ToUpper() == "ID") continue;
                stringBuilder.AppendFormat("{0} = @{1}, ", property.Name, property.Name);
            }
            stringBuilder.Remove(stringBuilder.Length - 2, 2);
            stringBuilder.AppendFormat(" WHERE Id = @Id");

            var database = new TDatabase();
            using (var command = database.CreateCommand(stringBuilder.ToString()))
            {
                foreach (var property in Columns)
                {
                    var paramName = string.Format("@{0}", property.Name);
                    command.Parameters.Add(database.CreateParameter(paramName, property.GetValue(this, null)));
                }

                _database.ExecuteCommand(command);
            }
        }

        public virtual void Remove()
        {
            if (Id > 0)
            {
                RemoveById(Id);
            }
        }

        public static void RemoveById(int id)
        {
            string query = string.Format("DELETE FROM {0} WHERE Id = @Id", FullTableName);

            var database = new TDatabase();
            using (var command = database.CreateCommand(query))
            {
                command.Parameters.Add(database.CreateParameter("@Id", id));
                _database.ExecuteCommand(command);
            }
        }

        public static void RemoveById(IList<T> entities)
        {
            foreach (var enity in entities)
            {
                RemoveById(enity.Id);
            }
        }

        public static T GetById(int id)
        {
            if (id > 0)
            {
                string query = string.Format("SELECT * FROM {0} WHERE Id = @Id", FullTableName);
                DataTable data;

                var database = new TDatabase();
                using (var command = database.CreateCommand(query))
                {
                    command.Parameters.Add(database.CreateParameter("@Id", id));
                    data = _database.GetData(command);
                }

                if (data.Rows != null && data.Rows.Count > 0)
                {
                    return Create(data.Rows[0]);
                }
            }

            return null;
        }

        public static int GetCountById(int id)
        {
            if (id > 0)
            {
                string query = string.Format("SELECT COUNT(1) FROM {0} WHERE Id = @Id", FullTableName);
                DataTable data;

                var database = new TDatabase();
                using (var command = database.CreateCommand(query))
                {
                    command.Parameters.Add(database.CreateParameter("@Id", id));
                    data = _database.GetData(command);
                }

                if (data != null && data.Rows != null && data.Rows.Count > 0)
                {
                    return data.Rows[0][0].ToInt();
                }
            }

            return 0;
        }

        public static int GetCount()
        {
            string query = string.Format("SELECT COUNT(1) FROM {0}", FullTableName);
            var data = _database.GetData(query);
            if (data != null && data.Rows != null && data.Rows.Count > 0)
            {
                return data.Rows[0][0].ToInt();
            }
            else
            {
                return 0;
            }
        }

        public static IList<T> GetFiltered(string filter)
        {
            if (string.IsNullOrEmpty(filter)) throw new ArgumentNullException("filter");

            string query = string.Format("SELECT * FROM {0} {1}", FullTableName, filter);
            var data = _database.GetData(query);
            return Create(data);
        }

        public static IList<T> GetAll()
        {
            string query = string.Format("SELECT * FROM {0}", FullTableName);
            var data = _database.GetData(query);
            return Create(data);
        }

        public static T Create(DataRow dataRow)
        {
            var result = new T();
            foreach (var property in typeof(T).GetProperties())
            {
                if (dataRow.Table.Columns.Contains(property.Name))
                {
                    if (dataRow[property.Name].GetType() == typeof(DBNull)) continue;
                    object value = dataRow[property.Name].To(property.PropertyType);
                    property.SetValue(result, value, null);
                }
            }

            return result;
        }

        public static IList<T> Create(DataTable dataTable)
        {
            var result = new List<T>();
            foreach (DataRow dataRow in dataTable.Rows)
            {
                result.Add(Create(dataRow));
            }

            return result;
        }

        public static bool IsTableExist()
        {
            return _database.IsTableExist(FullTableName);
        }

        public static void CreateTableIfNotExist()
        {
            if (!IsTableExist()) CreateTable();
        }

        public static string GetCreateTableQuery()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendFormat("CREATE TABLE {0}", FullTableName);
            stringBuilder.AppendLine();
            stringBuilder.Append("(");
            var columns = new SortedList();
            foreach (var propertyInfo in Columns)
            {
                var columnAttribute = (ColumnAttribute) propertyInfo.GetCustomAttributes(typeof (ColumnAttribute), true).First();
                var columnQuery = string.Format("{0},", new TDatabase().CreateColumnQuery(propertyInfo, columnAttribute));
                columns.Add(columnAttribute.Order, columnQuery);
            }

            foreach (DictionaryEntry e in columns)
            {
                stringBuilder.Append(e.Value.ToString());
            }
            stringBuilder.Remove(stringBuilder.Length - 1, 1);
            stringBuilder.Append(")");
            return stringBuilder.ToString();
        }

        public static void CreateTable()
        {
            _database.ExecuteQuery(GetCreateTableQuery());
        }

        public static void AddMissingColumns()
        {
            var query = string.Format(@"SELECT * FROM {0} WHERE 0=1", FullTableName);
            var data = _database.GetData(query);

            var stringBuilder = new StringBuilder();
            foreach (var column in Columns)
            {
                if (!data.Columns.Contains(column.Name))
                {
                    var columnAttribute = (ColumnAttribute)column.GetCustomAttributes(typeof(ColumnAttribute), true).First();
                    stringBuilder.AppendFormat("ALTER TABLE {0} ADD {1};", FullTableName, new TDatabase().CreateColumnQuery(column, columnAttribute));
                    stringBuilder.AppendLine();
                }
            }

            query = stringBuilder.ToString();
            if (!string.IsNullOrEmpty(query))
                _database.ExecuteQuery(query);
        }
        public static string GetPropertyName<TType>(Expression<Func<T, TType>> propertyExpression)
        {
            var body = (MemberExpression)propertyExpression.Body;
            return body.Member.Name;
        }

        public static IList<T> GetBy<TType>(Expression<Func<T, TType>> propertyExpression, TType value)
        {
            if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");

            var propertyName = ExpressionReflection.GetPropertyName(propertyExpression);
            string query = string.Format("SELECT * FROM {0} WHERE {1} = @value", FullTableName, propertyName);
            DataTable data;
            using (var command = _database.CreateCommand(query))
            {
                command.Parameters.Add(_database.CreateParameter("@value", value));
                data = _database.GetData(command);
            }

            if (data.Rows != null && data.Rows.Count > 0)
            {
                return Create(data);
            }
            else
            {
                return new List<T>();
            }
        }

        public static int GetCountBy<TType>(Expression<Func<T, TType>> propertyExpression, TType value)
        {
            if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");

            var propertyName = ExpressionReflection.GetPropertyName(propertyExpression);
            string query = string.Format("SELECT COUNT(1) FROM {0} WHERE {1} = @value", FullTableName, propertyName);
            using (var command = _database.CreateCommand(query))
            {
                command.Parameters.Add(_database.CreateParameter("@value", value));
                var data = _database.GetData(command);
                if (data != null && data.Rows != null && data.Rows.Count > 0)
                {
                    return data.Rows[0][0].ToInt();
                }
                else
                {
                    throw new Exception(string.Format(
                        "Не удалось получить количество записей по условию {0} = {1}", propertyName, value));
                }
            }
        }

        public static void RemoveBy<TType>(Expression<Func<T, TType>> propertyExpression, TType value)
        {
            if (propertyExpression == null) throw new ArgumentNullException("propertyExpression");

            var propertyName = ExpressionReflection.GetPropertyName(propertyExpression);
            string query = string.Format("DELETE FROM {0} WHERE {1} = @value", FullTableName, propertyName);

            using (var command = _database.CreateCommand(query))
            {
                command.Parameters.Add(_database.CreateParameter("@value", value));
                _database.ExecuteCommand(command);
            }
        }

        public static IList<T> GetBy<TType>(Dictionary<Expression<Func<T, TType>>, TType> filters)
        {
            // TODO Протестить
            if (filters == null) throw new ArgumentNullException("filters");

            int counter = 0;
            var queryBuilder = new StringBuilder();
            var parametersDictionary = new Dictionary<string, object>();
            queryBuilder.AppendFormat("SELECT * FROM {0} WHERE ", FullTableName);
            foreach (var filter in filters)
            {
                if (filter.Key == null || filter.Value == null) continue;
                string paramName = string.Format("@p{0}", counter++);
                var propertyName = ExpressionReflection.GetPropertyName(filter.Key);
                queryBuilder.AppendFormat("{0} = {1} AND ", propertyName, paramName);
                parametersDictionary.Add(paramName, filter.Value);
            }

            DataTable data;
            queryBuilder = queryBuilder.Remove(queryBuilder.Length - 5, 5);
            using (var command = _database.CreateCommand(queryBuilder.ToString()))
            {
                foreach (var param in parametersDictionary)
                {
                    command.Parameters.Add(_database.CreateParameter(param.Key, param.Value));
                }
                data = _database.GetData(command);
            }

            if (data.Rows != null && data.Rows.Count > 0)
            {
                return Create(data);
            }
            else
            {
                return new List<T>();
            }
        }

        public static int GetCountBy<TType>(Dictionary<Expression<Func<T, TType>>, TType> filters)
        {
            // TODO Протестить
            if (filters == null) throw new ArgumentNullException("filters");

            int counter = 0;
            var queryBuilder = new StringBuilder();
            var parametersDictionary = new Dictionary<string, object>();
            queryBuilder.AppendFormat("SELECT COUNT(1) FROM {0} WHERE ", FullTableName);
            foreach (var filter in filters)
            {
                if (filter.Key == null || filter.Value == null) continue;
                string paramName = string.Format("@p{0}", counter++);
                var propertyName = ExpressionReflection.GetPropertyName(filter.Key);
                queryBuilder.AppendFormat("{0} = {1} AND ", propertyName, paramName);
                parametersDictionary.Add(paramName, filter.Value);
            }

            queryBuilder = queryBuilder.Remove(queryBuilder.Length - 5, 5);
            using (var command = _database.CreateCommand(queryBuilder.ToString()))
            {
                foreach (var param in parametersDictionary)
                {
                    command.Parameters.Add(_database.CreateParameter(param.Key, param.Value));
                }

                var data = _database.GetData(command);
                if (data != null && data.Rows != null && data.Rows.Count > 0)
                {
                    return data.Rows[0][0].ToInt();
                }
                else
                {
                    throw new Exception(string.Format(
                        "Не удалось получить количество записей по условиям"));
                }
            }
        }

        public static void RemoveBy<TType>(Dictionary<Expression<Func<T, TType>>, TType> filters)
        {
            // TODO Протестить
            // TODO Криво работает ExpressionReflection.GetPropertyName с общими типами, например object

            if (filters == null) throw new ArgumentNullException("filters");

            int counter = 0;
            var queryBuilder = new StringBuilder();
            var parametersDictionary = new Dictionary<string, object>();
            queryBuilder.AppendFormat("DELETE FROM {0} WHERE ", FullTableName);
            foreach (var filter in filters)
            {
                if (filter.Key == null || filter.Value == null) continue;
                string paramName = string.Format("@p{0}", counter++);
                var propertyName = ExpressionReflection.GetPropertyName(filter.Key);
                queryBuilder.AppendFormat("{0} = {1} AND ", propertyName, paramName);
                parametersDictionary.Add(paramName, filter.Value);
            }

            queryBuilder = queryBuilder.Remove(queryBuilder.Length - 5, 5);
            using (var command = _database.CreateCommand(queryBuilder.ToString()))
            {
                foreach (var param in parametersDictionary)
                {
                    command.Parameters.Add(_database.CreateParameter(param.Key, param.Value));
                }

                _database.ExecuteCommand(command);
            }
        }

        #region Equals, GetHashCode

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as BaseEntity<T, TDatabase>;
            if (other == null)
            {
                return false;
            }
            return Equals(other);
        }

        public bool Equals(BaseEntity<T, TDatabase> other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            return (ReferenceEquals(this, other) || (other.Id == Id));
        }

        public override int GetHashCode()
        {
            return Id;
        }

        public static bool operator ==(BaseEntity<T, TDatabase> a, BaseEntity<T, TDatabase> b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(BaseEntity<T, TDatabase> a, BaseEntity<T, TDatabase> b)
        {
            return !Equals(a, b);
        }

        #endregion Equals, GetHashCode
    }
}