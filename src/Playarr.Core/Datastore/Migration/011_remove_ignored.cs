using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(11)]
    public class remove_ignored : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Delete.Column("Ignored").FromTable("Platforms");
            Delete.Column("Ignored").FromTable("Episodes");
        }
    }
}
