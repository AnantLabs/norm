# NORM is Lightweight .NET 2.0 [Active Record](http://martinfowler.com/eaaCatalog/activeRecord.html) ORM #


---


NORM lets you create object-relational mapping to databases in any .NET projects. Current version works with **SQL Server** and **SQLite** databases, but NORM can be extended to work with other databases as well.

NORM and NORM35 is distributed under an open-source license (MIT) that doesn't place any restrictions on what you can do with the source code.

# Features #

In addition to object-relational mapping NORM can also do **database migrations** (see examples).

**NORM35** is a version for .NET 3.5, NORM35 is currently in development. NORM35 will be much faster, and will support Expression Filters and enhanced SQL Server migrations.

# How to use #

Let’s look how to use NORM with SQLite database in C#.
First of all you need to create entities and configure a database.

```
    public class SQLiteDatabase : SQLite.SQLiteDatabase
    {
        public SQLiteDatabase()
            : base(ConfigurationManager.ConnectionStrings["SQLiteDatabase"].ConnectionString)
        {
        }
    }
```

Connection string in app.config:
```
  <connectionStrings>
    <add name="SQLiteDatabase" connectionString="Data Source=NormExample.db3;Version=3;New=True;" />
  </connectionStrings>
```

Create your entities like this:
```
    /// <summary>
    /// Column names should exactly match property names.
    /// Table should contain autoincrement integer primary key with name "Id" (inherited from BaseEntity).
    /// </summary>
    [Table("Bill")]
    public class Bill : BaseEntity<Bill, SQLiteDatabase>
    {
        public enum BillType
        {
            Simple = 0,
            Complex = 1
        }

        [Column]
        public string Name { get; set; }

        [Column(NotNull = true)]
        public DateTime Date { get; set; }

        [Column(NotNull = true)]
        public DateTime StartDate { get; set; }

        [Column(NotNull = true)]
        public DateTime EndDate { get; set; }

        [Column(NotNull = true)]
        public int UserId { get; set; }

        [Column(NotNull = true)]
        public BillType Type { get; set; }

        [Column]
        public int? NextBillId { get; set; }

        [Column]
        public int? PrevBillId { get; set; }

        public Bill NextBill
        {
            get { return NextBillId.HasValue ? GetById(NextBillId.Value) : null; }
        }
    }
```

Create table in a database if it doesn't exist:
```
        public void CreateTable()
        {
            Bill.CreateTableIfNotExist();
        }
```

The database NormExample.db3 will be created in bin folder. Table will be created with such SQL (Date type is TEXT in SQLite):
```
CREATE TABLE Bill( 
  Id INTEGER PRIMARY KEY AUTOINCREMENT, 
  Name TEXT, 
  Date TEXT NOT NULL, 
  StartDate TEXT NOT NULL, 
  EndDate TEXT NOT NULL, 
  UserId INTEGER NOT NULL, 
  Type INTEGER NOT NULL, 
  NextBillId INTEGER, 
  PrevBillId INTEGER
);
```

# CRUD Operations #

Let’s look how to code simple [CRUD](http://en.wikipedia.org/wiki/Create,_read,_update_and_delete) operations.

Create record:
```
        public void Create()
        {
            var bill = new Bill
                           {
                               Name = "NORM Example bill",
                               Date = DateTime.Now,
                               StartDate = DateTime.Now.AddMonths(-1),
                               EndDate = DateTime.Now.AddMonths(1),
                               UserId = 0,
                               NextBillId = null,
                               PrevBillId = null,
                               Type = Bill.BillType.Simple
                           };

            bill.Save();
        }
```

Retrieve record(s):
```
        public IEnumerable<Bill> Retrieve()
        {
            return Bill.GetAll();
        }

        public Bill Retrieve(int id)
        {
            return Bill.GetById(id);
        }

        public IEnumerable<Bill> RetrieveFiltered()
        {
            return Bill.GetFiltered("WHERE Id = 0");
        }

        public IEnumerable<Bill> RetrieveAdvanced(int id)
        {
            var database = new SQLiteDatabase();
            var query = string.Format("SELECT * FROM {0} WHERE Id = @Id", Bill.FullTableName);
            using (var command = database.CreateCommand(query))
            {
                command.Parameters.Add(database.CreateParameter("@Id", id));
                return Bill.Create(database.GetData(command));
            }
        }
```

Update record:
```
        public void Update(int id, string newName)
        {
            var bill = Bill.GetById(id);
            bill.Name = newName;
            bill.Save();
        }
```

Delete record:
```
        public void Delete(int id)
        {
            Bill.RemoveById(id);
        }

        public void Delete(Bill bill)
        {
            bill.Remove();
        }
```

# Database migrations (Database versioning) #

If you want to use database migrations you can do it with NORM. To use database migrations you need:
  1. Table to store current database version
  1. Migration objects (SQL scripts) marked with some version

In order to use database migrations, you should first create a new .NET Console Application, reference the NORM.dll assembly and set the database:
```
    public class SQLiteDatabase : SQLite.SQLiteDatabase
    {
        public SQLiteDatabase()
            : base(ConfigurationManager.ConnectionStrings["SQLiteDatabase"].ConnectionString)
        {
        }
    }
```

Next, you should map a table to store the current database version. If you already have Settings table (or smth like this) with columns Name and Value (at least) you can use it to store database version as shown here:
```
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
    }
```

Otherwise you can add a new entity named Settings, the table will be created automatically before applying first migration:
```
    [Table("Settings")]
    public class Settings : BaseEntity<Settings , SQLiteDatabase>, ISettingsEntity
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
```

Database version will be stored in the table 'Settings' as a record { Name: 'DBVersion', Value: '<numeric version>' }.

So, we are ready to write our first migration. Let me add new column 'Counter' to the table Bill. To do it I add new migration class:
```
    [Version(1)]
    public class V1 : BaseMigration
    {
        public override string Up
        {
            get
            {
                return @"ALTER TABLE Bill ADD Counter INTEGER;";
            }
        }
    }
```

To apply all migrations you should do this:
```
        static void Main(string[] args)
        {
            Console.WriteLine(@"Performing database migration...");
            new Migrator<SQLiteDatabase, Settings>().MigrateToLast();
        }
```

MigrateToLast method will apply all migrations in your project one by one (if its version higher than current database version) and will increment current version.

# Downloads #

Only binaries: [NORM.zip](http://code.google.com/p/norm/downloads/detail?name=NORM.zip)

Sources and examples: [NORM.sources.zip](http://code.google.com/p/norm/downloads/detail?name=NORM.sources.zip)