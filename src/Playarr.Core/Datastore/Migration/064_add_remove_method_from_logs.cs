using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(64)]
    public class remove_method_from_logs : PlayarrMigrationBase
    {
        protected override void LogDbUpgrade()
        {
            Delete.Column("Method").FromTable("Logs");
        }
    }
}
