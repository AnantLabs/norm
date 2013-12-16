using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NORM35.Entity;
using NORM35.Extensions;

namespace NORM35.Migration
{
    /// <summary>
    /// Позволяет проводить версионную миграцию структуры БД. Для использования 
    /// </summary>
    /// <typeparam name="TDatabase">Объект для работы с БД</typeparam>
    /// <typeparam name="TSettings">Таблица { Name, Value } для хранения текущей версии БД</typeparam>
    public class Migrator<TDatabase, TSettings>
        where TDatabase : Database, new()
        where TSettings : BaseEntity<TSettings, TDatabase>, ISettingsEntity, new()
    {
        /// <summary>
        /// Имя таблицы для хранения версии БД
        /// </summary>
        private readonly string _settingTableName;

        /// <summary>
        /// Имя сборки, содержащей миграции
        /// </summary>
        private readonly Assembly _migrationsAssembly = Assembly.GetEntryAssembly();

        /// <summary>
        /// Для работы с БД
        /// </summary>
        private readonly TDatabase _database = new TDatabase();

        public Migrator()
        {
            var settingAttribute = typeof(TSettings).GetCustomAttributes(typeof(TableAttribute), true).First() as TableAttribute;
            if (settingAttribute != null)
            {
                _settingTableName = settingAttribute.Name;
            }
            else
            {
                throw new Exception("Некорректная таблица для хранения версии БД");
            }
        }

        public Migrator(string migrationsAssemblyName)
            : this()
        {
            _migrationsAssembly = Assembly.Load(migrationsAssemblyName);
        }

        public Migrator(Assembly migrationsAssembly)
            : this()
        {
            _migrationsAssembly = migrationsAssembly;
        }

        /// <summary>
        /// Основной метод, поднимает версию БД до последней
        /// </summary>
        public virtual void MigrateToLast()
        {
            var currentVersion = GetCurrentVersion();
            var migrates = GetMigrations();
            foreach (var migrate in migrates.OrderBy(x => x.Key))
            {
                if (migrate.Key > currentVersion)
                {
                    var stringBuilder = new StringBuilder();
                    if (migrate.Value.UseTransaction) stringBuilder.AppendLine("BEGIN TRANSACTION T1;");
                    stringBuilder.AppendLine(migrate.Value.Up);
                    stringBuilder.AppendLine(GetUpdateCurrentVersionQuery(migrate.Key));
                    if (migrate.Value.UseTransaction) stringBuilder.AppendLine("COMMIT TRANSACTION T1;");
                    ExecuteMigration(stringBuilder);
                }
            }
        }

        /// <summary>
        /// Отправить миграцию на выполнение
        /// </summary>
        protected virtual void ExecuteMigration(StringBuilder stringBuilder)
        {
            var query = stringBuilder.ToString();
            if (!string.IsNullOrEmpty(query))
            {
                using (var command = _database.CreateCommand(query))
                {
                    command.CommandTimeout = 5*60;
                    _database.ExecuteCommand(command);
                }
            }
        }

        /// <summary>
        /// Получить все миграции
        /// </summary>
        public virtual Dictionary<int, BaseMigration> GetMigrations()
        {
            var migrationTypes = _migrationsAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(BaseMigration)) && !x.IsAbstract);
            var result = new Dictionary<int, BaseMigration>();
            foreach (var migrationType in migrationTypes)
            {
                var version =
                    migrationType.GetCustomAttributes(typeof (VersionAttribute), true).First() as VersionAttribute;
                var migration = Activator.CreateInstance(migrationType) as BaseMigration;
                if (version == null || migration == null) continue;
                result.Add(version.Version, migration);
            }

            return result;
        }

        /// <summary>
        /// Получить последнюю миграцию
        /// </summary>
        public virtual BaseMigration GetLastMigration()
        {
            var migrations = GetMigrations();
            return migrations[migrations.Max(x => x.Key)];
        }

        /// <summary>
        /// Получить текущую версию БД
        /// </summary>
        public virtual int GetCurrentVersion()
        {
            if (!_database.IsTableExist(_settingTableName)) CreateSettingsTable();

            var tsettings = new TSettings();
            string query = string.Format(
                "SELECT {0} FROM {1} WHERE {2} = 'DBVersion'",
                tsettings.ValueColumn,
                _settingTableName,
                tsettings.NameColumn);

            var data = _database.GetData(query);
            if (data.Rows.Count > 0)
            {
                return _database.GetData(query).Rows[0][0].ToInt();
            }
            else
            {
                AddVersionSetting();
                return 0;
            }
        }

        /// <summary>
        /// Создать таблицу для хранения версии БД
        /// </summary>
        private void CreateSettingsTable()
        {
            var columns = typeof(TSettings).
                GetProperty("Columns", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy).
                GetValue(null, null) as IEnumerable<PropertyInfo>;

            if (columns == null || columns.Count() < 2)
            {
                throw new Exception(
                    "Таблица для хранения версии БД не существует и не удалось ее создать, т.к. в классе настроек не заданы свойства Name и Value, помеченные атрибутами Column");
            }

            var method = typeof(TSettings).
                GetMethod("CreateTable", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy);

            method.Invoke(null, null);
        }

        /// <summary>
        /// Получить запрос на изменение версии БД
        /// </summary>
        protected virtual string GetUpdateCurrentVersionQuery(int dbVersion)
        {
            var tsettings = new TSettings();
            return string.Format(
                "UPDATE {0} SET {1} = {2} WHERE {3} = 'DBVersion'",
                _settingTableName,
                tsettings.ValueColumn,
                dbVersion,
                tsettings.NameColumn);
        }

        /// <summary>
        /// Добавить 0-версию БД
        /// </summary>
        protected virtual void AddVersionSetting()
        {
            var tsettings = new TSettings();
            string query = string.Format(
                "INSERT INTO {0} ({1}, {2}) VALUES ('DBVersion', 0)",
                _settingTableName,
                tsettings.NameColumn,
                tsettings.ValueColumn);

            _database.ExecuteQuery(query);
        }
    }
}