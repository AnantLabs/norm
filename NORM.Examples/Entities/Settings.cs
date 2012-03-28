using NORM.Entity;
using NORM.Examples.Databases;
using NORM.Migration;

namespace NORM.Examples.Entities
{
    [Table("Settings")]
    public class Settings : BaseEntity<Settings, SQLiteDatabase>, ISettingsEntity
    {
        public string NameColumn
        {
            get { return "Name"; }
        }

        public string ValueColumn
        {
            get { return "Value"; }
        }

        [Column(NotNull = true, Unique = true)]
        public string Name { get; set; }

        [Column]
        public string Value { get; set; }
    }
}
