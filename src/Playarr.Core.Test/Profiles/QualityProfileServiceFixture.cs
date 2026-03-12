using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using Moq;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.ImportLists;
using Playarr.Core.Lifecycle;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.Profiles
{
    [TestFixture]
    public class QualityProfileServiceFixture : CoreTest<QualityProfileService>
    {
        [SetUp]
        public void Setup()
        {
            Mocker.GetMock<IImportListFactory>()
                  .Setup(s => s.All())
                  .Returns(new List<ImportListDefinition>());
        }

        [Test]
        public void init_should_add_default_profiles()
        {
            Mocker.GetMock<ICustomFormatService>()
                  .Setup(s => s.All())
                  .Returns(new List<CustomFormat>());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityProfileRepository>()
                .Verify(v => v.Insert(It.IsAny<QualityProfile>()), Times.Exactly(3));
        }

        [Test]

        // This confirms that new profiles are added only if no other profiles exists.
        // We don't want to keep adding them back if a user deleted them on purpose.
        public void Init_should_skip_if_any_profiles_already_exist()
        {
            Mocker.GetMock<IQualityProfileRepository>()
                  .Setup(s => s.All())
                  .Returns(Builder<QualityProfile>.CreateListOfSize(2).Build().ToList());

            Subject.Handle(new ApplicationStartedEvent());

            Mocker.GetMock<IQualityProfileRepository>()
                .Verify(v => v.Insert(It.IsAny<QualityProfile>()), Times.Never());
        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_series()
        {
            var profile = Builder<QualityProfile>.CreateNew()
                                          .With(p => p.Id = 2)
                                          .Build();

            var gameList = Builder<Game>.CreateListOfSize(3)
                                            .Random(1)
                                            .With(c => c.QualityProfileId = profile.Id)
                                            .Build().ToList();

            Mocker.GetMock<IGameService>().Setup(c => c.GetAllGames()).Returns(gameList);
            Mocker.GetMock<IQualityProfileRepository>().Setup(c => c.Get(profile.Id)).Returns(profile);

            Assert.Throws<QualityProfileInUseException>(() => Subject.Delete(profile.Id));

            Mocker.GetMock<IQualityProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }

        [Test]
        public void should_delete_profile_if_not_assigned_to_series()
        {
            var gameList = Builder<Game>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.QualityProfileId = 2)
                                            .Build().ToList();

            Mocker.GetMock<IGameService>().Setup(c => c.GetAllGames()).Returns(gameList);

            Subject.Delete(1);

            Mocker.GetMock<IQualityProfileRepository>().Verify(c => c.Delete(1), Times.Once());
        }

        [Test]
        public void should_not_be_able_to_delete_profile_if_assigned_to_import_list()
        {
            var profile = Builder<QualityProfile>.CreateNew()
                                                 .With(p => p.Id = 1)
                                                 .Build();

            var gameList = Builder<Game>.CreateListOfSize(3)
                                            .All()
                                            .With(c => c.QualityProfileId = 2)
                                            .Build().ToList();

            var importLists = Builder<ImportListDefinition>.CreateListOfSize(3)
                                                           .Random(1)
                                                           .Build().ToList();

            Mocker.GetMock<IQualityProfileRepository>().Setup(c => c.Get(profile.Id)).Returns(profile);
            Mocker.GetMock<IGameService>().Setup(c => c.GetAllGames()).Returns(gameList);

            Mocker.GetMock<IImportListFactory>()
                  .Setup(s => s.All())
                  .Returns(importLists);

            Assert.Throws<QualityProfileInUseException>(() => Subject.Delete(1));

            Mocker.GetMock<IQualityProfileRepository>().Verify(c => c.Delete(It.IsAny<int>()), Times.Never());
        }
    }
}
