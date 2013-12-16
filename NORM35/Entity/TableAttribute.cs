using System;

namespace NORM35.Entity
{
    /// <summary>
    /// Аттрибут указывает, что класс соответствует таблице в БД
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class TableAttribute : Attribute
    {
        /// <summary>
        /// Имя таблицы
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Имя БД
        /// </summary>
        public string Database { get; set; }

        public TableAttribute(string  name)
        {
            Name = name;
        }

        public TableAttribute(string name, string database)
            : this(name)
        {
            Database = database;
        }
    }
}
