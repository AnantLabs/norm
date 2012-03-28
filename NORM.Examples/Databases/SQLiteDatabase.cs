using System.Configuration;

namespace NORM.Examples.Databases
{
    public class SQLiteDatabase : SQLite.SQLiteDatabase
    {
        public SQLiteDatabase()
            : base(ConfigurationManager.ConnectionStrings["SQLiteDatabase"].ConnectionString)
        {
        }
    }
}
