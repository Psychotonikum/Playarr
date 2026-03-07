using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(28)]
    public class add_blacklist_table : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("Blacklist")
                .WithColumn("SeriesId").AsInt32()
                .WithColumn("RomIds").AsString()
                .WithColumn("SourceTitle").AsString()
                .WithColumn("Quality").AsString()
                .WithColumn("Date").AsDateTime();
        }
    }
}
