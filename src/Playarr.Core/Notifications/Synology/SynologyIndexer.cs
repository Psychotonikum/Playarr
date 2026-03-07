using System.Collections.Generic;
using System.IO;
using FluentValidation.Results;
using Playarr.Common.EnvironmentInfo;
using Playarr.Common.Extensions;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles;
using Playarr.Core.Games;

namespace Playarr.Core.Notifications.Synology
{
    public class SynologyIndexer : NotificationBase<SynologyIndexerSettings>
    {
        private readonly ISynologyIndexerProxy _indexerProxy;
        private readonly ILocalizationService _localizationService;

        public SynologyIndexer(ISynologyIndexerProxy indexerProxy, ILocalizationService localizationService)
        {
            _indexerProxy = indexerProxy;
            _localizationService = localizationService;
        }

        public override string Link => "https://www.synology.com";
        public override string Name => "Synology Indexer";

        public override void OnDownload(DownloadMessage message)
        {
            if (Settings.UpdateLibrary)
            {
                foreach (var oldFile in message.OldFiles)
                {
                    var fullPath = Path.Combine(message.Game.Path, oldFile.RomFile.RelativePath);

                    _indexerProxy.DeleteFile(fullPath);
                }

                {
                    var fullPath = Path.Combine(message.Game.Path, message.RomFile.RelativePath);

                    _indexerProxy.AddFile(fullPath);
                }
            }
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            if (Settings.UpdateLibrary)
            {
                _indexerProxy.UpdateFolder(message.Game.Path);
            }
        }

        public override void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
            if (Settings.UpdateLibrary)
            {
                _indexerProxy.UpdateFolder(game.Path);
            }
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            if (Settings.UpdateLibrary)
            {
                var fullPath = Path.Combine(deleteMessage.Game.Path, deleteMessage.RomFile.RelativePath);
                _indexerProxy.DeleteFile(fullPath);
            }
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            if (Settings.UpdateLibrary)
            {
                _indexerProxy.UpdateFolder(message.Game.Path);
            }
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            if (deleteMessage.DeletedFiles)
            {
                if (Settings.UpdateLibrary)
                {
                    _indexerProxy.DeleteFolder(deleteMessage.Game.Path);
                }
            }
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestConnection());

            return new ValidationResult(failures);
        }

        protected virtual ValidationFailure TestConnection()
        {
            if (!OsInfo.IsLinux)
            {
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsSynologyValidationInvalidOs"));
            }

            if (!_indexerProxy.Test())
            {
                return new ValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsSynologyValidationTestFailed"));
            }

            return null;
        }
    }
}
