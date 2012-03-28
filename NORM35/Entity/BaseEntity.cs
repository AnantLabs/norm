using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Text;
using NORM;
using NORM.Extensions;
using NORM35.Reflection;

namespace NORM35.Entity
{
    public abstract class BaseEntity<T, TDatabase> : NORM.Entity.BaseEntity<T, TDatabase>
        where T : BaseEntity<T, TDatabase>, new()
        where TDatabase : Database, new()
    {
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

        //public new static T Create(DataRow dataRow)
        //{
        // Emit
        //}
    }
}
