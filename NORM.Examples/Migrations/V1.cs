using NORM.Migration;

namespace NORM.Examples.Migrations
{
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
}
