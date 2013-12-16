using System.Linq;

namespace NORM35.Migration
{
    public abstract class BaseMigration
    {
        public virtual bool UseTransaction
        {
            get { return false; }
        }

        public abstract string Up { get; }

        public virtual string Down
        {
            get { return string.Empty; }
        }

        public int Version
        {
            get
            {
                var version = (VersionAttribute)GetType().GetCustomAttributes(typeof(VersionAttribute), true).Single();
                return version.Version;
            }
        }
    }
}