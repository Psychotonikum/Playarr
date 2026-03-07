using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.RootFolders;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class GameFolderPathBuilderFixture : CoreTest<SeriesPathBuilder>
    {
        private Game _series;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateNew()
                                     .With(s => s.Title = "Game Title")
                                     .With(s => s.Path = @"C:\Test\TV\Game.Title".AsOsAgnostic())
                                     .With(s => s.RootFolderPath = null)
                                     .Build();
        }

        public void GivenGameFolderName(string name)
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetGameFolder(_series, null))
                  .Returns(name);
        }

        public void GivenExistingRootFolder(string rootFolder)
        {
            Mocker.GetMock<IRootFolderService>()
                  .Setup(s => s.GetBestRootFolderPath(It.IsAny<string>()))
                  .Returns(rootFolder);
        }

        [Test]
        public void should_create_new_series_path()
        {
            var rootFolder = @"C:\Test\TV2".AsOsAgnostic();

            GivenGameFolderName(_series.Title);
            _series.RootFolderPath = rootFolder;

            Subject.BuildPath(_series, false).Should().Be(Path.Combine(rootFolder, _series.Title));
        }

        [Test]
        public void should_reuse_existing_relative_folder_name()
        {
            var folderName = Path.GetFileName(_series.Path);
            var rootFolder = @"C:\Test\TV2".AsOsAgnostic();

            GivenExistingRootFolder(Path.GetDirectoryName(_series.Path));
            GivenGameFolderName(_series.Title);
            _series.RootFolderPath = rootFolder;

            Subject.BuildPath(_series, true).Should().Be(Path.Combine(rootFolder, folderName));
        }

        [Test]
        public void should_reuse_existing_relative_folder_structure()
        {
            var existingRootFolder = @"C:\Test\TV".AsOsAgnostic();
            var existingRelativePath = @"S\Game.Title";
            var rootFolder = @"C:\Test\TV2".AsOsAgnostic();

            GivenExistingRootFolder(existingRootFolder);
            GivenGameFolderName(_series.Title);
            _series.RootFolderPath = rootFolder;
            _series.Path = Path.Combine(existingRootFolder, existingRelativePath);

            Subject.BuildPath(_series, true).Should().Be(Path.Combine(rootFolder, existingRelativePath));
        }

        [Test]
        public void should_use_built_path_for_new_series()
        {
            var rootFolder = @"C:\Test\TV2".AsOsAgnostic();

            GivenGameFolderName(_series.Title);
            _series.RootFolderPath = rootFolder;
            _series.Path = null;

            Subject.BuildPath(_series, true).Should().Be(Path.Combine(rootFolder, _series.Title));
        }
    }
}
