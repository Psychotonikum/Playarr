using System;
using FluentMigrator;
using Playarr.Core.Datastore.Migration.Framework;

namespace Playarr.Core.Datastore.Migration
{
    [Migration(65)]
    public class make_scene_numbering_nullable : PlayarrMigrationBase
    {
        protected override void MainDbUpgrade()
        {
            Update.Table("Episodes").Set(new { AbsoluteEpisodeNumber = DBNull.Value }).Where(new { AbsoluteEpisodeNumber = 0 });
            Update.Table("Episodes").Set(new { SceneAbsoluteEpisodeNumber = DBNull.Value }).Where(new { SceneAbsoluteEpisodeNumber = 0 });
            Update.Table("Episodes").Set(new { SceneSeasonNumber = DBNull.Value, SceneEpisodeNumber = DBNull.Value }).Where(new { SceneSeasonNumber = 0, SceneEpisodeNumber = 0 });
        }
    }
}
