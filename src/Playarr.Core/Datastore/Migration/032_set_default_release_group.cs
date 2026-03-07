using System;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(32)]
    public class set_default_release_group : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("EpisodeFiles").Set(new { ReleaseGroup = "DRONE" }).Where(new { ReleaseGroup = DBNull.Value });
        }
    }
}
