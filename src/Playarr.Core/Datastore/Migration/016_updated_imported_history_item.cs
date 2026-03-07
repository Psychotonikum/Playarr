using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(16)]
    public class updated_imported_history_item : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Execute.Sql("UPDATE \"History\" SET \"Data\" = replace( \"Data\", '\"Path\"', '\"ImportedPath\"' ) WHERE \"EventType\" = 3");
        }
    }
}
