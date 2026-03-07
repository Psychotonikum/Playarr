using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FluentValidation.Results;
using NLog;
using Playarr.Common.Disk;
using Playarr.Common.Extensions;
using Playarr.Common.Processes;
using Playarr.Core.Configuration;
using Playarr.Core.HealthCheck;
using Playarr.Core.Localization;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Parser;
using Playarr.Core.Tags;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.CustomScript
{
    public class CustomScript : NotificationBase<CustomScriptSettings>
    {
        private readonly IConfigFileProvider _configFileProvider;
        private readonly IConfigService _configService;
        private readonly IDiskProvider _diskProvider;
        private readonly IProcessProvider _processProvider;
        private readonly ITagRepository _tagRepository;
        private readonly ILocalizationService _localizationService;
        private readonly Logger _logger;

        public CustomScript(IConfigFileProvider configFileProvider,
            IConfigService configService,
            IDiskProvider diskProvider,
            IProcessProvider processProvider,
            ITagRepository tagRepository,
            ILocalizationService localizationService,
            Logger logger)
        {
            _configFileProvider = configFileProvider;
            _configService = configService;
            _diskProvider = diskProvider;
            _processProvider = processProvider;
            _tagRepository = tagRepository;
            _localizationService = localizationService;
            _logger = logger;
        }

        public override string Name => _localizationService.GetLocalizedString("NotificationsCustomScriptSettingsName");

        public override string Link => "https://wiki.servarr.com/playarr/settings#connections";

        public override ProviderMessage Message => new ProviderMessage(_localizationService.GetLocalizedString("NotificationsCustomScriptSettingsProviderMessage", new Dictionary<string, object> { { "eventTypeTest", "Test" } }), ProviderMessageType.Warning);

        public override void OnGrab(GrabMessage message)
        {
            var game = message.Game;
            var remoteRom = message.Rom;
            var releaseGroup = remoteRom.ParsedRomInfo.ReleaseGroup;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Grab");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Playarr_Release_EpisodeCount", remoteRom.Roms.Count.ToString());
            environmentVariables.Add("Playarr_Release_PlatformNumber", remoteRom.Roms.First().SeasonNumber.ToString());
            environmentVariables.Add("Playarr_Release_RomNumbers", string.Join(",", remoteRom.Roms.Select(e => e.EpisodeNumber)));
            environmentVariables.Add("Playarr_Release_AbsoluteRomNumbers", string.Join(",", remoteRom.Roms.Select(e => e.AbsoluteEpisodeNumber)));
            environmentVariables.Add("Playarr_Release_EpisodeAirDates", string.Join(",", remoteRom.Roms.Select(e => e.AirDate)));
            environmentVariables.Add("Playarr_Release_EpisodeAirDatesUtc", string.Join(",", remoteRom.Roms.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Playarr_Release_RomTitles", string.Join("|", remoteRom.Roms.Select(e => e.Title)));
            environmentVariables.Add("Playarr_Release_EpisodeOverviews", string.Join("|", remoteRom.Roms.Select(e => e.Overview)));
            environmentVariables.Add("Playarr_Release_FinaleTypes", string.Join("|", remoteRom.Roms.Select(e => e.FinaleType)));
            environmentVariables.Add("Playarr_Release_Title", remoteRom.Release.Title);
            environmentVariables.Add("Playarr_Release_Indexer", remoteRom.Release.Indexer ?? string.Empty);
            environmentVariables.Add("Playarr_Release_Size", remoteRom.Release.Size.ToString());
            environmentVariables.Add("Playarr_Release_Quality", remoteRom.ParsedRomInfo.Quality.Quality.Name);
            environmentVariables.Add("Playarr_Release_QualityVersion", remoteRom.ParsedRomInfo.Quality.Revision.Version.ToString());
            environmentVariables.Add("Playarr_Release_ReleaseGroup", releaseGroup ?? string.Empty);
            environmentVariables.Add("Playarr_Release_IndexerFlags", remoteRom.Release.IndexerFlags.ToString());
            environmentVariables.Add("Playarr_Download_Client", message.DownloadClientName ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Client_Type", message.DownloadClientType ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Playarr_Release_CustomFormat", string.Join("|", remoteRom.CustomFormats));
            environmentVariables.Add("Playarr_Release_CustomFormatScore", remoteRom.CustomFormatScore.ToString());
            environmentVariables.Add("Playarr_Release_ReleaseType", remoteRom.ParsedRomInfo.ReleaseType.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var game = message.Game;
            var romFile = message.RomFile;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Download");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Playarr_IsUpgrade", message.OldFiles.Any().ToString());
            environmentVariables.Add("Playarr_RomFile_Id", romFile.Id.ToString());
            environmentVariables.Add("Playarr_RomFile_EpisodeCount", romFile.Roms.Value.Count.ToString());
            environmentVariables.Add("Playarr_RomFile_RelativePath", romFile.RelativePath);
            environmentVariables.Add("Playarr_RomFile_Path", Path.Combine(game.Path, romFile.RelativePath));
            environmentVariables.Add("Playarr_RomFile_RomIds", string.Join(",", romFile.Roms.Value.Select(e => e.Id)));
            environmentVariables.Add("Playarr_RomFile_PlatformNumber", romFile.SeasonNumber.ToString());
            environmentVariables.Add("Playarr_RomFile_RomNumbers", string.Join(",", romFile.Roms.Value.Select(e => e.EpisodeNumber)));
            environmentVariables.Add("Playarr_RomFile_EpisodeAirDates", string.Join(",", romFile.Roms.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Playarr_RomFile_EpisodeAirDatesUtc", string.Join(",", romFile.Roms.Value.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Playarr_RomFile_RomTitles", string.Join("|", romFile.Roms.Value.Select(e => e.Title)));
            environmentVariables.Add("Playarr_RomFile_EpisodeOverviews", string.Join("|", romFile.Roms.Value.Select(e => e.Overview)));
            environmentVariables.Add("Playarr_RomFile_FinaleTypes", string.Join("|", romFile.Roms.Value.Select(e => e.FinaleType)));
            environmentVariables.Add("Playarr_RomFile_Quality", romFile.Quality.Quality.Name);
            environmentVariables.Add("Playarr_RomFile_QualityVersion", romFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Playarr_RomFile_ReleaseGroup", romFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Playarr_RomFile_SceneName", romFile.SceneName ?? string.Empty);
            environmentVariables.Add("Playarr_RomFile_SourcePath", sourcePath);
            environmentVariables.Add("Playarr_RomFile_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Playarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Playarr_RomFile_MediaInfo_AudioChannels", MediaInfoFormatter.FormatAudioChannels(romFile.MediaInfo.PrimaryAudioStream).ToString());
            environmentVariables.Add("Playarr_RomFile_MediaInfo_AudioCodec", MediaInfoFormatter.FormatAudioCodec(romFile.MediaInfo.PrimaryAudioStream, null));
            environmentVariables.Add("Playarr_RomFile_MediaInfo_AudioLanguages", romFile.MediaInfo.AudioStreams?.Select(l => l.Language).Distinct().ConcatToString(" / "));
            environmentVariables.Add("Playarr_RomFile_MediaInfo_Languages", romFile.MediaInfo.AudioStreams?.Select(l => l.Language).ConcatToString(" / "));
            environmentVariables.Add("Playarr_RomFile_MediaInfo_Height", romFile.MediaInfo.Height.ToString());
            environmentVariables.Add("Playarr_RomFile_MediaInfo_Width", romFile.MediaInfo.Width.ToString());
            environmentVariables.Add("Playarr_RomFile_MediaInfo_Subtitles", romFile.MediaInfo.SubtitleStreams?.Select(l => l.Language).ConcatToString(" / "));
            environmentVariables.Add("Playarr_RomFile_MediaInfo_VideoCodec", MediaInfoFormatter.FormatVideoCodec(romFile.MediaInfo, null));
            environmentVariables.Add("Playarr_RomFile_MediaInfo_VideoDynamicRangeType", MediaInfoFormatter.FormatVideoDynamicRangeType(romFile.MediaInfo));
            environmentVariables.Add("Playarr_RomFile_CustomFormat", string.Join("|", message.RomInfo.CustomFormats));
            environmentVariables.Add("Playarr_RomFile_CustomFormatScore", message.RomInfo.CustomFormatScore.ToString());
            environmentVariables.Add("Playarr_Release_Indexer", message.Release?.Indexer);
            environmentVariables.Add("Playarr_Release_Size", message.Release?.Size.ToString());
            environmentVariables.Add("Playarr_Release_Title", message.Release?.Title);
            environmentVariables.Add("Playarr_Release_ReleaseType", message.Release?.ReleaseType.ToString() ?? string.Empty);

            if (message.OldFiles.Any())
            {
                environmentVariables.Add("Playarr_DeletedRelativePaths", string.Join("|", message.OldFiles.Select(e => e.RomFile.RelativePath)));
                environmentVariables.Add("Playarr_DeletedPaths", string.Join("|", message.OldFiles.Select(e => Path.Combine(game.Path, e.RomFile.RelativePath))));
                environmentVariables.Add("Playarr_DeletedDateAdded", string.Join("|", message.OldFiles.Select(e => e.RomFile.DateAdded)));
                environmentVariables.Add("Playarr_DeletedRecycleBinPaths", string.Join("|", message.OldFiles.Select(e => e.RecycleBinPath ?? string.Empty)));
            }

            ExecuteScript(environmentVariables);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            var game = message.Game;
            var roms = message.Roms;
            var romFiles = message.RomFiles;
            var sourcePath = message.SourcePath;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Download");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Playarr_RomFile_Ids", string.Join("|", romFiles.Select(f => f.Id)));
            environmentVariables.Add("Playarr_RomFile_Count", message.RomFiles.Count.ToString());
            environmentVariables.Add("Playarr_RomFile_RelativePaths", string.Join("|", romFiles.Select(f => f.RelativePath)));
            environmentVariables.Add("Playarr_RomFile_Paths", string.Join("|", romFiles.Select(f => Path.Combine(game.Path, f.RelativePath))));
            environmentVariables.Add("Playarr_RomFile_RomIds", string.Join(",", roms.Select(e => e.Id)));
            environmentVariables.Add("Playarr_RomFile_PlatformNumber", roms.First().SeasonNumber.ToString());
            environmentVariables.Add("Playarr_RomFile_RomNumbers", string.Join(",", roms.Select(e => e.EpisodeNumber)));
            environmentVariables.Add("Playarr_RomFile_EpisodeAirDates", string.Join(",", roms.Select(e => e.AirDate)));
            environmentVariables.Add("Playarr_RomFile_EpisodeAirDatesUtc", string.Join(",", roms.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Playarr_RomFile_RomTitles", string.Join("|", roms.Select(e => e.Title)));
            environmentVariables.Add("Playarr_RomFile_EpisodeOverviews", string.Join("|", roms.Select(e => e.Overview)));
            environmentVariables.Add("Playarr_RomFile_FinaleTypes", string.Join("|", roms.Select(e => e.FinaleType)));
            environmentVariables.Add("Playarr_RomFile_Qualities", string.Join("|", romFiles.Select(f => f.Quality.Quality.Name)));
            environmentVariables.Add("Playarr_RomFile_QualityVersions", string.Join("|", romFiles.Select(f => f.Quality.Revision.Version)));
            environmentVariables.Add("Playarr_RomFile_ReleaseGroups", string.Join("|", romFiles.Select(f => f.ReleaseGroup)));
            environmentVariables.Add("Playarr_RomFile_SceneNames", string.Join("|", romFiles.Select(f => f.SceneName)));
            environmentVariables.Add("Playarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Playarr_Release_Group", message.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Playarr_Release_Quality", message.ReleaseQuality.Quality.Name);
            environmentVariables.Add("Playarr_Release_QualityVersion", message.ReleaseQuality.Revision.Version.ToString());
            environmentVariables.Add("Playarr_Release_Indexer", message.Release?.Indexer ?? string.Empty);
            environmentVariables.Add("Playarr_Release_Size", message.Release?.Size.ToString() ?? string.Empty);
            environmentVariables.Add("Playarr_Release_Title", message.Release?.Title ?? string.Empty);

            // Prefer the release type from the release, otherwise use the first imported file (useful for untracked manual imports)
            environmentVariables.Add("Playarr_Release_ReleaseType", message.Release == null ? message.RomFiles.First().ReleaseType.ToString() : message.Release.ReleaseType.ToString());
            environmentVariables.Add("Playarr_SourcePath", sourcePath);
            environmentVariables.Add("Playarr_SourceFolder", Path.GetDirectoryName(sourcePath));
            environmentVariables.Add("Playarr_DestinationPath", message.DestinationPath);
            environmentVariables.Add("Playarr_DestinationFolder", Path.GetDirectoryName(message.DestinationPath));

            ExecuteScript(environmentVariables);
        }

        public override void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "Rename");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Playarr_RomFile_Ids", string.Join(",", renamedFiles.Select(e => e.RomFile.Id)));
            environmentVariables.Add("Playarr_RomFile_RelativePaths", string.Join("|", renamedFiles.Select(e => e.RomFile.RelativePath)));
            environmentVariables.Add("Playarr_RomFile_Paths", string.Join("|", renamedFiles.Select(e => Path.Combine(game.Path, e.RomFile.RelativePath))));
            environmentVariables.Add("Playarr_RomFile_PreviousRelativePaths", string.Join("|", renamedFiles.Select(e => e.PreviousRelativePath)));
            environmentVariables.Add("Playarr_RomFile_PreviousPaths", string.Join("|", renamedFiles.Select(e => e.PreviousPath)));

            ExecuteScript(environmentVariables);
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var game = deleteMessage.Game;
            var romFile = deleteMessage.RomFile;

            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "RomFileDelete");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Playarr_RomFile_Id", romFile.Id.ToString());
            environmentVariables.Add("Playarr_RomFile_EpisodeCount", romFile.Roms.Value.Count.ToString());
            environmentVariables.Add("Playarr_RomFile_RelativePath", romFile.RelativePath);
            environmentVariables.Add("Playarr_RomFile_Path", Path.Combine(game.Path, romFile.RelativePath));
            environmentVariables.Add("Playarr_RomFile_RomIds", string.Join(",", romFile.Roms.Value.Select(e => e.Id)));
            environmentVariables.Add("Playarr_RomFile_PlatformNumber", romFile.SeasonNumber.ToString());
            environmentVariables.Add("Playarr_RomFile_RomNumbers", string.Join(",", romFile.Roms.Value.Select(e => e.EpisodeNumber)));
            environmentVariables.Add("Playarr_RomFile_EpisodeAirDates", string.Join(",", romFile.Roms.Value.Select(e => e.AirDate)));
            environmentVariables.Add("Playarr_RomFile_EpisodeAirDatesUtc", string.Join(",", romFile.Roms.Value.Select(e => e.AirDateUtc)));
            environmentVariables.Add("Playarr_RomFile_RomTitles", string.Join("|", romFile.Roms.Value.Select(e => e.Title)));
            environmentVariables.Add("Playarr_RomFile_EpisodeOverviews", string.Join("|", romFile.Roms.Value.Select(e => e.Overview)));
            environmentVariables.Add("Playarr_RomFile_Quality", romFile.Quality.Quality.Name);
            environmentVariables.Add("Playarr_RomFile_QualityVersion", romFile.Quality.Revision.Version.ToString());
            environmentVariables.Add("Playarr_RomFile_ReleaseGroup", romFile.ReleaseGroup ?? string.Empty);
            environmentVariables.Add("Playarr_RomFile_SceneName", romFile.SceneName ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var game = message.Game;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "SeriesAdd");
            AddGameVariables(environmentVariables, game);

            ExecuteScript(environmentVariables);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var game = deleteMessage.Game;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "SeriesDelete");
            AddGameVariables(environmentVariables, game);

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "HealthIssue");

            environmentVariables.Add("Playarr_Health_Issue_Level", Enum.GetName(typeof(HealthCheckResult), healthCheck.Type));
            environmentVariables.Add("Playarr_Health_Issue_Message", healthCheck.Message);
            environmentVariables.Add("Playarr_Health_Issue_Type", healthCheck.Source.Name);
            environmentVariables.Add("Playarr_Health_Issue_Wiki", healthCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "HealthRestored");

            environmentVariables.Add("Playarr_Health_Restored_Level", Enum.GetName(typeof(HealthCheckResult), previousCheck.Type));
            environmentVariables.Add("Playarr_Health_Restored_Message", previousCheck.Message);
            environmentVariables.Add("Playarr_Health_Restored_Type", previousCheck.Source.Name);
            environmentVariables.Add("Playarr_Health_Restored_Wiki", previousCheck.WikiUrl.ToString() ?? string.Empty);

            ExecuteScript(environmentVariables);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "ApplicationUpdate");

            environmentVariables.Add("Playarr_Update_Message", updateMessage.Message);
            environmentVariables.Add("Playarr_Update_NewVersion", updateMessage.NewVersion.ToString());
            environmentVariables.Add("Playarr_Update_PreviousVersion", updateMessage.PreviousVersion.ToString());

            ExecuteScript(environmentVariables);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var game = message.Game;
            var environmentVariables = new StringDictionary();

            AddInstanceVariables(environmentVariables, "ManualInteractionRequired");
            AddGameVariables(environmentVariables, game);

            environmentVariables.Add("Playarr_Download_Client", message.DownloadClientInfo?.Name ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Client_Type", message.DownloadClientInfo?.Type ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Id", message.DownloadId ?? string.Empty);
            environmentVariables.Add("Playarr_Download_Size", message.TrackedDownload.DownloadItem.TotalSize.ToString());
            environmentVariables.Add("Playarr_Download_Title", message.TrackedDownload.DownloadItem.Title);

            ExecuteScript(environmentVariables);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            if (!_diskProvider.FileExists(Settings.Path))
            {
                failures.Add(new PlayarrValidationFailure("Path", _localizationService.GetLocalizedString("NotificationsCustomScriptValidationFileDoesNotExist")));
            }

            if (failures.Empty())
            {
                try
                {
                    var environmentVariables = new StringDictionary();
                    environmentVariables.Add("Playarr_EventType", "Test");
                    environmentVariables.Add("Playarr_InstanceName", _configFileProvider.InstanceName);
                    environmentVariables.Add("Playarr_ApplicationUrl", _configService.ApplicationUrl);

                    var processOutput = ExecuteScript(environmentVariables);

                    if (processOutput.ExitCode != 0)
                    {
                        failures.Add(new PlayarrValidationFailure(string.Empty, $"Script exited with code: {processOutput.ExitCode}"));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                    failures.Add(new PlayarrValidationFailure(string.Empty, ex.Message));
                }
            }

            return new ValidationResult(failures);
        }

        private ProcessOutput ExecuteScript(StringDictionary environmentVariables)
        {
            _logger.Debug("Executing external script: {0}", Settings.Path);

            var processOutput = _processProvider.StartAndCapture(Settings.Path, Settings.Arguments, environmentVariables);

            _logger.Debug("Executed external script: {0} - Status: {1}", Settings.Path, processOutput.ExitCode);
            _logger.Debug("Script Output: \r\n{0}", string.Join("\r\n", processOutput.Lines));

            return processOutput;
        }

        private bool ValidatePathParent(string possibleParent, string path)
        {
            return possibleParent.IsParentPath(path);
        }

        private List<string> GetTagLabels(Game game)
        {
            if (game == null)
            {
                return new List<string>();
            }

            return _tagRepository.GetTags(game.Tags)
                .Select(s => s.Label)
                .Where(l => l.IsNotNullOrWhiteSpace())
                .OrderBy(l => l)
                .ToList();
        }

        private void AddInstanceVariables(StringDictionary environmentVariables, string eventType)
        {
            environmentVariables.Add("Playarr_EventType", eventType);
            environmentVariables.Add("Playarr_InstanceName", _configFileProvider.InstanceName);
            environmentVariables.Add("Playarr_ApplicationUrl", _configService.ApplicationUrl);
        }

        private void AddGameVariables(StringDictionary environmentVariables, Game game)
        {
            environmentVariables.Add("Playarr_Series_Id", game.Id.ToString());
            environmentVariables.Add("Playarr_Series_Title", game.Title);
            environmentVariables.Add("Playarr_Series_TitleSlug", game.TitleSlug);
            environmentVariables.Add("Playarr_Series_Path", game.Path);
            environmentVariables.Add("Playarr_Series_IgdbId", game.TvdbId.ToString());
            environmentVariables.Add("Playarr_Series_RawgId", game.RawgId.ToString());
            environmentVariables.Add("Playarr_Series_TmdbId", game.TmdbId.ToString());
            environmentVariables.Add("Playarr_Series_ImdbId", game.ImdbId ?? string.Empty);
            environmentVariables.Add("Playarr_Series_Type", game.SeriesType.ToString());
            environmentVariables.Add("Playarr_Series_Year", game.Year.ToString());
            environmentVariables.Add("Playarr_Series_OriginalCountry", game.OriginalCountry);
            environmentVariables.Add("Playarr_Series_OriginalLanguage", IsoLanguages.Get(game.OriginalLanguage).ThreeLetterCode);
            environmentVariables.Add("Playarr_Series_Genres", string.Join("|", game.Genres));
            environmentVariables.Add("Playarr_Series_Tags", string.Join("|", GetTagLabels(game)));
        }
    }
}
