using System.Collections.Generic;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.MediaFiles;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;

namespace Playarr.Core.Extras.Metadata
{
    public interface IMetadata : IProvider
    {
        string GetFilenameAfterMove(Game game, RomFile romFile, MetadataFile metadataFile);
        MetadataFile FindMetadataFile(Game game, string path);
        MetadataFileResult SeriesMetadata(Game game, SeriesMetadataReason reason);
        MetadataFileResult EpisodeMetadata(Game game, RomFile romFile);
        List<ImageFileResult> GameImages(Game game);
        List<ImageFileResult> SeasonImages(Game game, Platform platform);
        List<ImageFileResult> EpisodeImages(Game game, RomFile romFile);
    }

    public enum SeriesMetadataReason
    {
        Scan,
        EpisodeFolderCreated,
        EpisodesImported
    }
}
