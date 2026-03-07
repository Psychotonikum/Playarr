using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using Playarr.Common.Reflection;
using Playarr.Core.Authentication;
using Playarr.Core.AutoTagging.Specifications;
using Playarr.Core.Blocklisting;
using Playarr.Core.Configuration;
using Playarr.Core.CustomFilters;
using Playarr.Core.CustomFormats;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.Datastore.Converters;
using Playarr.Core.Download;
using Playarr.Core.Download.History;
using Playarr.Core.Download.Pending;
using Playarr.Core.Extras.Metadata;
using Playarr.Core.Extras.Metadata.Files;
using Playarr.Core.Extras.Others;
using Playarr.Core.Extras.Subtitles;
using Playarr.Core.History;
using Playarr.Core.ImportLists;
using Playarr.Core.ImportLists.Exclusions;
using Playarr.Core.Indexers;
using Playarr.Core.Instrumentation;
using Playarr.Core.Jobs;
using Playarr.Core.Languages;
using Playarr.Core.MediaFiles;
using Playarr.Core.Messaging.Commands;
using Playarr.Core.Notifications;
using Playarr.Core.Organizer;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles;
using Playarr.Core.Profiles.Delay;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Profiles.Releases;
using Playarr.Core.Qualities;
using Playarr.Core.RemotePathMappings;
using Playarr.Core.RootFolders;
using Playarr.Core.Tags;
using Playarr.Core.ThingiProvider;
using Playarr.Core.Games;
using Playarr.Core.Update.History;
using static Dapper.SqlMapper;

namespace Playarr.Core.Datastore
{
    public static class TableMapping
    {
        static TableMapping()
        {
            Mapper = new TableMapper();
        }

        public static TableMapper Mapper { get; private set; }

        public static void Map()
        {
            RegisterMappers();

            Mapper.Entity<Config>("Config").RegisterModel();

            Mapper.Entity<RootFolder>("RootFolders").RegisterModel()
                  .Ignore(r => r.Accessible)
                  .Ignore(r => r.IsEmpty)
                  .Ignore(r => r.FreeSpace)
                  .Ignore(r => r.TotalSpace);

            Mapper.Entity<ScheduledTask>("ScheduledTasks").RegisterModel()
                  .Ignore(i => i.Priority);

            Mapper.Entity<IndexerDefinition>("Indexers").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.Enable)
                  .Ignore(i => i.Protocol)
                  .Ignore(i => i.SupportsRss)
                  .Ignore(i => i.SupportsSearch);

            Mapper.Entity<ImportListDefinition>("ImportLists").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.ListType)
                  .Ignore(i => i.MinRefreshInterval)
                  .Ignore(i => i.Enable);

            Mapper.Entity<ImportListItemInfo>("ImportListItems").RegisterModel()
                   .Ignore(i => i.ImportList)
                   .Ignore(i => i.Platforms);

            Mapper.Entity<NotificationDefinition>("Notifications").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(i => i.SupportsOnGrab)
                  .Ignore(i => i.SupportsOnDownload)
                  .Ignore(i => i.SupportsOnImportComplete)
                  .Ignore(i => i.SupportsOnUpgrade)
                  .Ignore(i => i.SupportsOnRename)
                  .Ignore(i => i.SupportsOnSeriesAdd)
                  .Ignore(i => i.SupportsOnSeriesDelete)
                  .Ignore(i => i.SupportsOnRomFileDelete)
                  .Ignore(i => i.SupportsOnRomFileDeleteForUpgrade)
                  .Ignore(i => i.SupportsOnHealthIssue)
                  .Ignore(i => i.SupportsOnHealthRestored)
                  .Ignore(i => i.SupportsOnApplicationUpdate)
                  .Ignore(i => i.SupportsOnManualInteractionRequired);

            Mapper.Entity<MetadataDefinition>("Metadata").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(d => d.Tags);

            Mapper.Entity<DownloadClientDefinition>("DownloadClients").RegisterModel()
                  .Ignore(x => x.ImplementationName)
                  .Ignore(d => d.Protocol);

            Mapper.Entity<SceneMapping>("SceneMappings").RegisterModel();

            Mapper.Entity<EpisodeHistory>("History").RegisterModel();

            Mapper.Entity<Game>("Series").RegisterModel()
                  .Ignore(s => s.RootFolderPath)
                  .HasOne(s => s.QualityProfile, s => s.QualityProfileId);

            Mapper.Entity<RomFile>("EpisodeFiles").RegisterModel()
                  .HasOne(f => f.Game, f => f.SeriesId)
                  .LazyLoad(x => x.Roms,
                            (db, parent) => db.Query<Rom>(new SqlBuilder(db.DatabaseType).Where<Rom>(c => c.EpisodeFileId == parent.Id)).ToList(),
                            t => t.Id > 0)
                  .Ignore(f => f.Path);

            Mapper.Entity<Rom>("Episodes").RegisterModel()
                  .Ignore(e => e.GameTitle)
                  .Ignore(e => e.Game)
                  .Ignore(e => e.HasFile)
                  .Ignore(e => e.AbsoluteRomNumberAdded)
                  .HasOne(s => s.RomFile, s => s.EpisodeFileId);

            Mapper.Entity<QualityDefinition>("QualityDefinitions").RegisterModel()
                  .Ignore(d => d.GroupName)
                  .Ignore(d => d.Weight)
                  .Ignore(d => d.MinSize)
                  .Ignore(d => d.MaxSize)
                  .Ignore(d => d.PreferredSize);

            Mapper.Entity<CustomFormat>("CustomFormats").RegisterModel();

            Mapper.Entity<QualityProfile>("QualityProfiles").RegisterModel();
            Mapper.Entity<Log>("Logs").RegisterModel();
            Mapper.Entity<NamingConfig>("NamingConfig").RegisterModel();
            Mapper.Entity<Blocklist>("Blocklist").RegisterModel();
            Mapper.Entity<MetadataFile>("MetadataFiles").RegisterModel();
            Mapper.Entity<SubtitleFile>("SubtitleFiles").RegisterModel();
            Mapper.Entity<OtherExtraFile>("ExtraFiles").RegisterModel();

            Mapper.Entity<PendingRelease>("PendingReleases").RegisterModel()
                  .Ignore(e => e.RemoteEpisode);

            Mapper.Entity<RemotePathMapping>("RemotePathMappings").RegisterModel();
            Mapper.Entity<Tag>("Tags").RegisterModel();
            Mapper.Entity<ReleaseProfile>("ReleaseProfiles").RegisterModel();

            Mapper.Entity<DelayProfile>("DelayProfiles").RegisterModel();
            Mapper.Entity<User>("Users").RegisterModel();
            Mapper.Entity<CommandModel>("Commands").RegisterModel()
                .Ignore(c => c.Message);

            Mapper.Entity<IndexerStatus>("IndexerStatus").RegisterModel();
            Mapper.Entity<DownloadClientStatus>("DownloadClientStatus").RegisterModel();
            Mapper.Entity<ImportListStatus>("ImportListStatus").RegisterModel();
            Mapper.Entity<NotificationStatus>("NotificationStatus").RegisterModel();

            Mapper.Entity<CustomFilter>("CustomFilters").RegisterModel();

            Mapper.Entity<DownloadHistory>("DownloadHistory").RegisterModel();

            Mapper.Entity<UpdateHistory>("UpdateHistory").RegisterModel();
            Mapper.Entity<ImportListExclusion>("ImportListExclusions").RegisterModel();

            Mapper.Entity<AutoTagging.AutoTag>("AutoTagging").RegisterModel();
        }

        private static void RegisterMappers()
        {
            RegisterEmbeddedConverter();
            RegisterProviderSettingConverter();

            SqlMapper.RemoveTypeMap(typeof(DateTime));
            SqlMapper.AddTypeHandler(new DapperUtcConverter());
            SqlMapper.AddTypeHandler(new DapperQualityIntConverter());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<QualityProfileQualityItem>>(new QualityIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ProfileFormatItem>>(new CustomFormatIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<ICustomFormatSpecification>>(new CustomFormatSpecificationListConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<IAutoTaggingSpecification>>(new AutoTaggingSpecificationConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<QualityModel>(new QualityIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<Dictionary<string, string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<IDictionary<string, string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<int>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<KeyValuePair<string, int>>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<KeyValuePair<string, int>>());
            SqlMapper.AddTypeHandler(new DapperLanguageIntConverter());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<List<Language>>(new LanguageIntConverter()));
            SqlMapper.AddTypeHandler(new StringListConverter<List<string>>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ParsedRomInfo>(new QualityIntConverter(), new LanguageIntConverter()));
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<ReleaseInfo>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<PendingReleaseAdditionalInfo>());
            SqlMapper.AddTypeHandler(new EmbeddedDocumentConverter<HashSet<int>>());
            SqlMapper.AddTypeHandler(new OsPathConverter());
            SqlMapper.RemoveTypeMap(typeof(Guid));
            SqlMapper.RemoveTypeMap(typeof(Guid?));
            SqlMapper.AddTypeHandler(new GuidConverter());
            SqlMapper.RemoveTypeMap(typeof(TimeSpan));
            SqlMapper.RemoveTypeMap(typeof(TimeSpan?));
            SqlMapper.AddTypeHandler(new TimeSpanConverter());
            SqlMapper.AddTypeHandler(new CommandConverter());
            SqlMapper.AddTypeHandler(new SystemVersionConverter());
        }

        private static void RegisterProviderSettingConverter()
        {
            var settingTypes = typeof(IProviderConfig).Assembly.ImplementationsOf<IProviderConfig>()
                .Where(x => !x.ContainsGenericParameters);

            var providerSettingConverter = new ProviderSettingConverter();
            foreach (var embeddedType in settingTypes)
            {
                SqlMapper.AddTypeHandler(embeddedType, providerSettingConverter);
            }
        }

        private static void RegisterEmbeddedConverter()
        {
            var embeddedTypes = typeof(IEmbeddedDocument).Assembly.ImplementationsOf<IEmbeddedDocument>();

            var embeddedConverterDefinition = typeof(EmbeddedDocumentConverter<>).GetGenericTypeDefinition();
            var genericListDefinition = typeof(List<>).GetGenericTypeDefinition();

            foreach (var embeddedType in embeddedTypes)
            {
                var embeddedListType = genericListDefinition.MakeGenericType(embeddedType);

                RegisterEmbeddedConverter(embeddedType, embeddedConverterDefinition);
                RegisterEmbeddedConverter(embeddedListType, embeddedConverterDefinition);
            }
        }

        private static void RegisterEmbeddedConverter(Type embeddedType, Type embeddedConverterDefinition)
        {
            var embeddedConverterType = embeddedConverterDefinition.MakeGenericType(embeddedType);
            var converter = (ITypeHandler)Activator.CreateInstance(embeddedConverterType);

            SqlMapper.AddTypeHandler(embeddedType, converter);
        }
    }
}
