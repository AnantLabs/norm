using System.Text;

namespace NORM35.Entity
{
    public abstract class BaseImportableEntity<T, TDatabase> : BaseEntity<T, TDatabase>
        where T : BaseEntity<T, TDatabase>, new()
        where TDatabase: Database, new()
    {
        public virtual void Import()
        {
            var database = new TDatabase();
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(database.CreateIdentityInsertQuery(FullTableName, true));
            stringBuilder.Append("INSERT INTO");
            stringBuilder.AppendFormat(" {0} ", FullTableName);

            var stringFields = new StringBuilder();
            var stringValues = new StringBuilder();
            foreach (var property in Columns)
            {
                stringFields.AppendFormat("{0}, ", property.Name);
                stringValues.AppendFormat("@{0}, ", property.Name);
            }
            stringFields.Remove(stringFields.Length - 2, 2);
            stringValues.Remove(stringValues.Length - 2, 2);
            stringBuilder.AppendFormat("({0}) ", stringFields);
            stringBuilder.AppendFormat("VALUES ({0})", stringValues);
            stringBuilder.AppendLine();
            stringBuilder.Append(database.CreateIdentityInsertQuery(FullTableName, false));

            using (var command = database.CreateCommand(stringBuilder.ToString()))
            {
                foreach (var property in Columns)
                {
                    var paramName = string.Format("@{0}", property.Name);
                    command.Parameters.Add(database.CreateParameter(paramName, property.GetValue(this, null)));
                }

                Id = _database.ExecuteInsert(command);
            }
        }
    }
}