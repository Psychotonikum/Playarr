using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(84)]
    public class update_quality_minmax_size : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("QualityDefinitions").AlterColumn("MinSize").AsDouble().Nullable();
            Alter.Table("QualityDefinitions").AlterColumn("MaxSize").AsDouble().Nullable();

            Execute.Sql("UPDATE \"QualityDefinitions\" SET \"MaxSize\" = NULL WHERE \"Quality\" = 10 OR \"MaxSize\" = 0");
        }
    }

    public class QualityDefinition84
    {
        public int Id { get; set; }
        public int Quality { get; set; }
        public string Title { get; set; }
        public int? MinSize { get; set; }
        public int? MaxSize { get; set; }
    }
}
