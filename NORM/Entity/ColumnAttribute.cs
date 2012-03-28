using System;

namespace NORM.Entity
{
    /// <summary>
    /// Аттрибут указывает, что свойство используется в качестве столбца БД
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
    public class ColumnAttribute : Attribute
    {
        private static int nextOrder = 100;

        public ColumnAttribute()
        {
            Size = 200;
            Order = GenerateOrderNumber();
        }

        /// <summary>
        /// Тип данных
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Флаг [not null].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [not null]; otherwise, <c>false</c>.
        /// </value>
        public bool NotNull { get; set; }

        /// <summary>
        /// Флаг [unique].
        /// </summary>
        public bool Unique { get; set; }

        /// <summary>
        /// Флаг [Primary].
        /// </summary>
        public bool Primary { get; set; }

        /// <summary>
        /// Флаг [AutoIncrement].
        /// </summary>
        public bool AutoIncrement { get; set; }

        /// <summary>
        /// Значение по умолчанию в формате БД
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// Размер поля (применимо для строковых значений)
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Номер столбца в таблшице БД
        /// </summary>
        public int Order { get; set; }

        private static int GenerateOrderNumber()
        {
            return nextOrder++;
        }
    }
}