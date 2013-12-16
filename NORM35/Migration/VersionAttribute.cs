using System;

namespace NORM35.Migration
{
    /// <summary>
    /// Аттрибут указывает версию БД
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class VersionAttribute : Attribute
    {
        /// <summary>
        /// Номер версии
        /// </summary>
        public int Version { get; set; }

        public VersionAttribute(int version)
        {
            Version = version;
        }
    }
}
