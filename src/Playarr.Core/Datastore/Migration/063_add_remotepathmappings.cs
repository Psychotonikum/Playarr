using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(63)]
    public class add_remotepathmappings : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Create.TableForModel("RemotePathMappings")
                  .WithColumn("Host").AsString()
                  .WithColumn("RemotePath").AsString()
                  .WithColumn("LocalPath").AsString();
        }
    }
}
