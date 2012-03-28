namespace NORM.Migration
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
    }
}