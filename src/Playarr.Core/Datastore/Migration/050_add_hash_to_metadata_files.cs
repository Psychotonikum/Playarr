using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(50)]
    public class add_hash_to_metadata_files : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Alter.Table("MetadataFiles").AddColumn("Hash").AsString().Nullable();
        }
    }
}
