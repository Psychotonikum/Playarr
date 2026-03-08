using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentValidation.Results;
using Playarr.Common.Extensions;
using Playarr.Core.Configuration;
using Playarr.Core.Localization;
using Playarr.Core.MediaCover;
using Playarr.Core.MediaFiles;
using Playarr.Core.MediaFiles.MediaInfo;
using Playarr.Core.Notifications.Discord.Payloads;
using Playarr.Core.Games;
using Playarr.Core.Validation;

namespace Playarr.Core.Notifications.Discord
{
    public class Discord : NotificationBase<DiscordSettings>
    {
        private readonly IDiscordProxy _proxy;
        private readonly IConfigFileProvider _configFileProvider;
        private readonly ILocalizationService _localizationService;

        public Discord(IDiscordProxy proxy, IConfigFileProvider configFileProvider, ILocalizationService localizationService)
        {
            _proxy = proxy;
            _configFileProvider = configFileProvider;
            _localizationService = localizationService;
        }

        public override string Name => "Discord";
        public override string Link => "https://support.discordapp.com/hc/en-us/articles/228383668-Intro-to-Webhooks";

        public override void OnGrab(GrabMessage message)
        {
            var game = message.Game;
            var roms = message.Rom.Roms;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = $"http://theigdb.com/?tab=game&id={game.IgdbId}",
                Description = "Rom Grabbed",
                Title = GetTitle(game, roms),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.GrabFields.Contains((int)DiscordGrabFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.GrabFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordGrabFieldType)field)
                {
                    case DiscordGrabFieldType.Overview:
                        var overview = roms.First().Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordGrabFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = game.Ratings.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case DiscordGrabFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = game.Genres.Take(5).Join(", ");
                        break;
                    case DiscordGrabFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.Quality.Quality.Name;
                        break;
                    case DiscordGrabFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.Rom.ParsedRomInfo.ReleaseGroup;
                        break;
                    case DiscordGrabFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.Rom.Release.Size);
                        discordField.Inline = true;
                        break;
                    case DiscordGrabFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = string.Format("```{0}```", message.Rom.Release.Title);
                        break;
                    case DiscordGrabFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(game);
                        break;
                    case DiscordGrabFieldType.CustomFormats:
                        discordField.Name = "Custom Formats";
                        discordField.Value = string.Join("|", message.Rom.CustomFormats);
                        break;
                    case DiscordGrabFieldType.CustomFormatScore:
                        discordField.Name = "Custom Format Score";
                        discordField.Value = message.Rom.CustomFormatScore.ToString();
                        break;
                    case DiscordGrabFieldType.Indexer:
                        discordField.Name = "Indexer";
                        discordField.Value = message.Rom.Release.Indexer;
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnDownload(DownloadMessage message)
        {
            var game = message.Game;
            var roms = message.RomFile.Roms.Value;
            var isUpgrade = message.OldFiles.Count > 0;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = $"http://theigdb.com/?tab=game&id={game.IgdbId}",
                Description = isUpgrade ? "Rom Upgraded" : "Rom Imported",
                Title = GetTitle(game, roms),
                Color = isUpgrade ? (int)DiscordColors.Upgrade : (int)DiscordColors.Success,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.ImportFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordImportFieldType)field)
                {
                    case DiscordImportFieldType.Overview:
                        var overview = roms.First().Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordImportFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = game.Ratings.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case DiscordImportFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = game.Genres.Take(5).Join(", ");
                        break;
                    case DiscordImportFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.RomFile.Quality.Quality.Name;
                        break;
                    case DiscordImportFieldType.Codecs:
                        discordField.Name = "Codecs";
                        discordField.Inline = true;
                        discordField.Value = string.Format("{0} / {1} {2}",
                            MediaInfoFormatter.FormatVideoCodec(message.RomFile.MediaInfo, null),
                            MediaInfoFormatter.FormatAudioCodec(message.RomFile.MediaInfo.PrimaryAudioStream, null),
                            MediaInfoFormatter.FormatAudioChannels(message.RomFile.MediaInfo.PrimaryAudioStream));
                        break;
                    case DiscordImportFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.RomFile.ReleaseGroup;
                        break;
                    case DiscordImportFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.RomFile.Size);
                        discordField.Inline = true;
                        break;
                    case DiscordImportFieldType.Languages:
                        discordField.Name = "Languages";
                        discordField.Value = message.RomFile.MediaInfo.AudioStreams?.Select(l => l.Language).ConcatToString("/");
                        break;
                    case DiscordImportFieldType.Subtitles:
                        discordField.Name = "Subtitles";
                        discordField.Value = message.RomFile.MediaInfo.SubtitleStreams?.Select(l => l.Language).ConcatToString("/");
                        break;
                    case DiscordImportFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = string.Format("```{0}```", message.RomFile.SceneName);
                        break;
                    case DiscordImportFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(game);
                        break;
                    case DiscordImportFieldType.CustomFormats:
                        discordField.Name = "Custom Formats";
                        discordField.Value = string.Join("|", message.RomInfo.CustomFormats);
                        break;
                    case DiscordImportFieldType.CustomFormatScore:
                        discordField.Name = "Custom Format Score";
                        discordField.Value = message.RomInfo.CustomFormatScore.ToString();
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnImportComplete(ImportCompleteMessage message)
        {
            var game = message.Game;
            var roms = message.Roms;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = $"http://theigdb.com/?tab=game&id={game.IgdbId}",
                Description = "Import Complete",
                Title = GetTitle(game, roms),
                Color = (int)DiscordColors.Success,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.ImportFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordImportFieldType)field)
                {
                    case DiscordImportFieldType.Overview:
                        var overview = roms.First().Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordImportFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = game.Ratings.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case DiscordImportFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = game.Genres.Take(5).Join(", ");
                        break;
                    case DiscordImportFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.ReleaseQuality.Quality.Name;
                        break;
                    case DiscordImportFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.ReleaseGroup;
                        break;
                    case DiscordImportFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.Release?.Size ?? message.RomFiles.Sum(f => f.Size));
                        discordField.Inline = true;
                        break;
                    case DiscordImportFieldType.Release:
                        discordField.Name = "Release";
                        discordField.Value = $"```{message.Release?.Title ?? message.SourceTitle}```";
                        break;
                    case DiscordImportFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(game);
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnRename(Game game, List<RenamedRomFile> renamedFiles)
        {
            var attachments = new List<Embed>
            {
                new()
                {
                    Title = game.Title,
                }
            };

            var payload = CreatePayload("Renamed", attachments);

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnRomFileDelete(EpisodeDeleteMessage deleteMessage)
        {
            var game = deleteMessage.Game;
            var roms = deleteMessage.RomFile.Roms;
            var deletedFile = deleteMessage.RomFile.Path;
            var reason = deleteMessage.Reason;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = $"http://theigdb.com/?tab=game&id={game.IgdbId}",
                Title = GetTitle(game, roms),
                Description = "Rom Deleted",
                Color = (int)DiscordColors.Danger,
                Fields = new List<DiscordField>
                {
                    new() { Name = "Reason", Value = reason.ToString() },
                    new() { Name = "File name", Value = string.Format("```{0}```", deletedFile) }
                },
                Timestamp = DateTime.UtcNow.ToString("O"),
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnSeriesAdd(SeriesAddMessage message)
        {
            var game = message.Game;
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = $"http://theigdb.com/?tab=game&id={game.IgdbId}",
                Title = game.Title,
                Description = "Game Added",
                Color = (int)DiscordColors.Success,
                Fields = new List<DiscordField> { new() { Name = "Links", Value = GetLinksString(game) } }
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnSeriesDelete(SeriesDeleteMessage deleteMessage)
        {
            var game = deleteMessage.Game;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = $"http://theigdb.com/?tab=game&id={game.IgdbId}",
                Title = game.Title,
                Description = deleteMessage.DeletedFilesMessage,
                Color = (int)DiscordColors.Danger,
                Fields = new List<DiscordField> { new() { Name = "Links", Value = GetLinksString(game) } }
            };

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ImportFields.Contains((int)DiscordImportFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = game.Images.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthIssue(HealthCheck.HealthCheck healthCheck)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Title = healthCheck.Source.Name,
                Description = healthCheck.Message,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Color = healthCheck.Type == HealthCheck.HealthCheckResult.Warning ? (int)DiscordColors.Warning : (int)DiscordColors.Danger
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnHealthRestored(HealthCheck.HealthCheck previousCheck)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Title = "Health Issue Resolved: " + previousCheck.Source.Name,
                Description = $"The following issue is now resolved: {previousCheck.Message}",
                Timestamp = DateTime.UtcNow.ToString("O"),
                Color = (int)DiscordColors.Success
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnApplicationUpdate(ApplicationUpdateMessage updateMessage)
        {
            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Title = APPLICATION_UPDATE_TITLE,
                Timestamp = DateTime.UtcNow.ToString("O"),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>()
                {
                    new()
                    {
                        Name = "Previous Version",
                        Value = updateMessage.PreviousVersion.ToString()
                    },
                    new()
                    {
                        Name = "New Version",
                        Value = updateMessage.NewVersion.ToString()
                    }
                },
            };

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override void OnManualInteractionRequired(ManualInteractionRequiredMessage message)
        {
            var game = message.Game;
            var roms = message.Rom.Roms;

            var embed = new Embed
            {
                Author = new DiscordAuthor
                {
                    Name = Settings.Author.IsNullOrWhiteSpace() ? _configFileProvider.InstanceName : Settings.Author,
                    IconUrl = "https://raw.githubusercontent.com/Playarr/Playarr/develop/Logo/256.png"
                },
                Url = game?.IgdbId > 0 ? $"http://theigdb.com/?tab=game&id={game.IgdbId}" : null,
                Description = "Manual interaction needed",
                Title = GetTitle(game, roms),
                Color = (int)DiscordColors.Standard,
                Fields = new List<DiscordField>(),
                Timestamp = DateTime.UtcNow.ToString("O")
            };

            if (Settings.ManualInteractionFields.Contains((int)DiscordManualInteractionFieldType.Poster))
            {
                embed.Thumbnail = new DiscordImage
                {
                    Url = game?.Images?.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Poster)?.RemoteUrl
                };
            }

            if (Settings.ManualInteractionFields.Contains((int)DiscordManualInteractionFieldType.Fanart))
            {
                embed.Image = new DiscordImage
                {
                    Url = game?.Images?.FirstOrDefault(x => x.CoverType == MediaCoverTypes.Fanart)?.RemoteUrl
                };
            }

            foreach (var field in Settings.ManualInteractionFields)
            {
                var discordField = new DiscordField();

                switch ((DiscordManualInteractionFieldType)field)
                {
                    case DiscordManualInteractionFieldType.Overview:
                        var overview = roms.FirstOrDefault()?.Overview ?? "";
                        discordField.Name = "Overview";
                        discordField.Value = overview.Length <= 300 ? overview : $"{overview.AsSpan(0, 300)}...";
                        break;
                    case DiscordManualInteractionFieldType.Rating:
                        discordField.Name = "Rating";
                        discordField.Value = game?.Ratings?.Value.ToString(CultureInfo.InvariantCulture);
                        break;
                    case DiscordManualInteractionFieldType.Genres:
                        discordField.Name = "Genres";
                        discordField.Value = game?.Genres.Take(5).Join(", ");
                        break;
                    case DiscordManualInteractionFieldType.Quality:
                        discordField.Name = "Quality";
                        discordField.Inline = true;
                        discordField.Value = message.Quality?.Quality?.Name;
                        break;
                    case DiscordManualInteractionFieldType.Group:
                        discordField.Name = "Group";
                        discordField.Value = message.Rom?.ParsedRomInfo?.ReleaseGroup;
                        break;
                    case DiscordManualInteractionFieldType.Size:
                        discordField.Name = "Size";
                        discordField.Value = BytesToString(message.TrackedDownload.DownloadItem.TotalSize);
                        discordField.Inline = true;
                        break;
                    case DiscordManualInteractionFieldType.DownloadTitle:
                        discordField.Name = "Download";
                        discordField.Value = $"```{message.TrackedDownload.DownloadItem.Title}```";
                        break;
                    case DiscordManualInteractionFieldType.Links:
                        discordField.Name = "Links";
                        discordField.Value = GetLinksString(game);
                        break;
                }

                if (discordField.Name.IsNotNullOrWhiteSpace() && discordField.Value.IsNotNullOrWhiteSpace())
                {
                    embed.Fields.Add(discordField);
                }
            }

            var payload = CreatePayload(null, new List<Embed> { embed });

            _proxy.SendPayload(payload, Settings);
        }

        public override ValidationResult Test()
        {
            var failures = new List<ValidationFailure>();

            failures.AddIfNotNull(TestMessage());

            return new ValidationResult(failures);
        }

        public ValidationFailure TestMessage()
        {
            try
            {
                var message = $"Test message from Playarr posted at {DateTime.Now}";
                var payload = CreatePayload(message);

                _proxy.SendPayload(payload, Settings);
            }
            catch (DiscordException ex)
            {
                return new PlayarrValidationFailure(string.Empty, _localizationService.GetLocalizedString("NotificationsValidationUnableToSendTestMessage", new Dictionary<string, object> { { "exceptionMessage", ex.Message } }));
            }

            return null;
        }

        private DiscordPayload CreatePayload(string message, List<Embed> embeds = null)
        {
            var avatar = Settings.Avatar;

            var payload = new DiscordPayload
            {
                Username = Settings.Username,
                Content = message,
                Embeds = embeds
            };

            if (avatar.IsNotNullOrWhiteSpace())
            {
                payload.AvatarUrl = avatar;
            }

            if (Settings.Username.IsNotNullOrWhiteSpace())
            {
                payload.Username = Settings.Username;
            }

            return payload;
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
            {
                return "0 " + suf[0];
            }

            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return string.Format("{0} {1}", (Math.Sign(byteCount) * num).ToString(), suf[place]);
        }

        private string GetLinksString(Game game)
        {
            if (game == null)
            {
                return null;
            }

            var links = new List<string>
            {
                $"[The IGDB](https://theigdb.com/?tab=game&id={game.IgdbId})",
                $"[Trakt](https://trakt.tv/search/igdb/{game.IgdbId}?id_type=show)"
            };

            if (game.ImdbId.IsNotNullOrWhiteSpace())
            {
                links.Add($"[IMDB](https://imdb.com/title/{game.ImdbId}/)");
            }

            return string.Join(" / ", links);
        }

        private string GetTitle(Game game, List<Rom> roms)
        {
            if (game == null)
            {
                return null;
            }

            if (roms.Empty())
            {
                return game.Title.Replace("`", "\\`");
            }

            if (game.SeriesType == GameTypes.Daily)
            {
                var rom = roms.First();

                return $"{game.Title} - {rom.AirDate} - {rom.Title}".Replace("`", "\\`");
            }

            var romNumbers = string.Concat(roms.Select(e => $"x{e.EpisodeNumber:00}"));

            var romTitles = string.Join(" + ", roms.Select(e => e.Title));

            var title = $"{game.Title} - {roms.First().PlatformNumber}{romNumbers} - {romTitles}".Replace("`", "\\`");

            return title.Length > 256 ? $"{title.AsSpan(0, 253).TrimEnd('\\')}..." : title;
        }
    }
}
