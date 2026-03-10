using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DataAugmentation.Scene;
using Playarr.Core.DecisionEngine;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.IndexerSearch.Definitions;
using Playarr.Core.Parser;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class DownloadDecisionMakerFixture : CoreTest<DownloadDecisionMaker>
    {
        private List<ReleaseInfo> _reports;
        private RemoteRom _remoteRom;

        private Mock<IDownloadDecisionEngineSpecification> _pass1;
        private Mock<IDownloadDecisionEngineSpecification> _pass2;
        private Mock<IDownloadDecisionEngineSpecification> _pass3;

        private Mock<IDownloadDecisionEngineSpecification> _fail1;
        private Mock<IDownloadDecisionEngineSpecification> _fail2;
        private Mock<IDownloadDecisionEngineSpecification> _fail3;

        private Mock<IDownloadDecisionEngineSpecification> _failDelayed1;

        [SetUp]
        public void Setup()
        {
            _pass1 = new Mock<IDownloadDecisionEngineSpecification>();
            _pass2 = new Mock<IDownloadDecisionEngineSpecification>();
            _pass3 = new Mock<IDownloadDecisionEngineSpecification>();

            _fail1 = new Mock<IDownloadDecisionEngineSpecification>();
            _fail2 = new Mock<IDownloadDecisionEngineSpecification>();
            _fail3 = new Mock<IDownloadDecisionEngineSpecification>();

            _failDelayed1 = new Mock<IDownloadDecisionEngineSpecification>();

            _pass1.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Accept);
            _pass2.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Accept);
            _pass3.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Accept);

            _fail1.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Reject(DownloadRejectionReason.Unknown, "fail1"));
            _fail2.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Reject(DownloadRejectionReason.Unknown, "fail2"));
            _fail3.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Reject(DownloadRejectionReason.Unknown, "fail3"));

            _failDelayed1.Setup(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>())).Returns(DownloadSpecDecision.Reject(DownloadRejectionReason.MinimumAgeDelay, "failDelayed1"));
            _failDelayed1.SetupGet(c => c.Priority).Returns(SpecificationPriority.Disk);

            _reports = new List<ReleaseInfo> { new ReleaseInfo { Title = "The.Office.S03E115.DVDRip.XviD-OSiTV" } };
            _remoteRom = new RemoteRom
            {
                Game = new Game(),
                Roms = new List<Rom> { new Rom() }
            };

            Mocker.GetMock<IParsingService>()
                  .Setup(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()))
                  .Returns(_remoteRom);
        }

        private void GivenSpecifications(params Mock<IDownloadDecisionEngineSpecification>[] mocks)
        {
            Mocker.SetConstant<IEnumerable<IDownloadDecisionEngineSpecification>>(mocks.Select(c => c.Object));
        }

        [Test]
        public void should_call_all_specifications()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            Subject.GetRssDecision(_reports).ToList();

            _fail1.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
            _fail2.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
            _fail3.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
            _pass1.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
            _pass2.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
            _pass3.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
        }

        [Test]
        public void should_call_delayed_specifications_if_non_delayed_passed()
        {
            GivenSpecifications(_pass1, _failDelayed1);

            Subject.GetRssDecision(_reports).ToList();
            _failDelayed1.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Once());
        }

        [Test]
        public void should_not_call_delayed_specifications_if_non_delayed_failed()
        {
            GivenSpecifications(_fail1, _failDelayed1);

            Subject.GetRssDecision(_reports).ToList();

            _failDelayed1.Verify(c => c.IsSatisfiedBy(_remoteRom, It.IsAny<ReleaseDecisionInformation>()), Times.Never());
        }

        [Test]
        public void should_return_rejected_if_single_specs_fail()
        {
            GivenSpecifications(_fail1);

            var result = Subject.GetRssDecision(_reports);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_rejected_if_one_of_specs_fail()
        {
            GivenSpecifications(_pass1, _fail1, _pass2, _pass3);

            var result = Subject.GetRssDecision(_reports);

            result.Single().Approved.Should().BeFalse();
        }

        [Test]
        public void should_return_pass_if_all_specs_pass()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            var result = Subject.GetRssDecision(_reports);

            result.Single().Approved.Should().BeTrue();
        }

        [Test]
        public void should_have_same_number_of_rejections_as_specs_that_failed()
        {
            GivenSpecifications(_pass1, _pass2, _pass3, _fail1, _fail2, _fail3);

            var result = Subject.GetRssDecision(_reports);
            result.Single().Rejections.Should().HaveCount(3);
        }

        [Test]
        public void should_not_attempt_to_map_episode_if_not_parsable()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            _reports[0].Title = "Not parsable";

            Subject.GetRssDecision(_reports).ToList();

            Mocker.GetMock<IParsingService>().Verify(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()), Times.Never());

            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
        }

        [Test]
        public void should_not_attempt_to_map_episode_if_series_title_is_blank()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            _reports[0].Title = "1937 - Snow White and the Seven Dwarves";

            var results = Subject.GetRssDecision(_reports).ToList();

            Mocker.GetMock<IParsingService>().Verify(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()), Times.Never());

            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());

            results.Should().BeEmpty();
        }

        [Test]
        public void should_return_rejected_result_for_unparsable_search()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);
            _reports[0].Title = "1937 - Snow White and the Seven Dwarves";

            Subject.GetSearchDecision(_reports, new SingleEpisodeSearchCriteria()).ToList();

            Mocker.GetMock<IParsingService>().Verify(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()), Times.Never());

            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
        }

        [Test]
        public void should_not_attempt_to_make_decision_if_series_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _remoteRom.Game = null;

            Subject.GetRssDecision(_reports);

            _pass1.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass2.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
            _pass3.Verify(c => c.IsSatisfiedBy(It.IsAny<RemoteRom>(), It.IsAny<ReleaseDecisionInformation>()), Times.Never());
        }

        [Test]
        public void broken_report_shouldnt_blowup_the_process()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>().Setup(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()))
                     .Throws<TestException>();

            _reports = new List<ReleaseInfo>
                {
                    new ReleaseInfo { Title = "The.Office.S03E115.DVDRip.XviD-OSiTV" },
                    new ReleaseInfo { Title = "The.Office.S03E115.DVDRip.XviD-OSiTV" },
                    new ReleaseInfo { Title = "The.Office.S03E115.DVDRip.XviD-OSiTV" }
                };

            Subject.GetRssDecision(_reports);

            Mocker.GetMock<IParsingService>().Verify(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()), Times.Exactly(_reports.Count));

            ExceptionVerification.ExpectedErrors(3);
        }

        [Test]
        public void should_return_unknown_series_rejection_if_series_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _remoteRom.Game = null;

            var result = Subject.GetRssDecision(_reports);

            result.Should().HaveCount(1);
        }

        [Test]
        public void should_only_include_reports_for_requested_episodes()
        {
            var game = Builder<Game>.CreateNew().Build();

            var roms = Builder<Rom>.CreateListOfSize(2)
                .All()
                .With(v => v.GameId, game.Id)
                .With(v => v.Game, game)
                .With(v => v.PlatformNumber, 1)
                .With(v => v.ScenePlatformNumber, 2)
                .BuildList();

            var criteria = new SeasonSearchCriteria { Roms = roms.Take(1).ToList(), PlatformNumber = 1 };

            var reports = roms.Select(v =>
                new ReleaseInfo()
                {
                    Title = string.Format("{0}.S{1:00}E{2:00}.720p.WEB-DL-DRONE", game.Title, v.ScenePlatformNumber, v.SceneEpisodeNumber)
                }).ToList();

            Mocker.GetMock<IParsingService>()
                .Setup(v => v.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()))
                .Returns<ParsedRomInfo, int, int, string, SearchCriteriaBase>((p, _, _, _, _) =>
                    new RemoteRom
                    {
                        DownloadAllowed = true,
                        ParsedRomInfo = p,
                        Game = game,
                        Roms = roms.Where(v => v.SceneEpisodeNumber == p.RomNumbers.First()).ToList()
                    });

            Mocker.SetConstant<IEnumerable<IDownloadDecisionEngineSpecification>>(new List<IDownloadDecisionEngineSpecification>
            {
                Mocker.Resolve<Playarr.Core.DecisionEngine.Specifications.Search.EpisodeRequestedSpecification>()
            });

            var decisions = Subject.GetSearchDecision(reports, criteria);

            var approvedDecisions = decisions.Where(v => v.Approved).ToList();

            approvedDecisions.Count.Should().Be(1);
        }

        [Test]
        public void should_not_allow_download_if_series_is_unknown()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _remoteRom.Game = null;

            var result = Subject.GetRssDecision(_reports);

            result.Should().HaveCount(1);

            result.First().RemoteRom.DownloadAllowed.Should().BeFalse();
        }

        [Test]
        public void should_not_allow_download_if_no_episodes_found()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            _remoteRom.Roms = new List<Rom>();

            var result = Subject.GetRssDecision(_reports);

            result.Should().HaveCount(1);

            result.First().RemoteRom.DownloadAllowed.Should().BeFalse();
        }

        [Test]
        public void should_return_a_decision_when_exception_is_caught()
        {
            GivenSpecifications(_pass1);

            Mocker.GetMock<IParsingService>().Setup(c => c.Map(It.IsAny<ParsedRomInfo>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SearchCriteriaBase>()))
                     .Throws<TestException>();

            _reports = new List<ReleaseInfo>
                {
                    new ReleaseInfo { Title = "The.Office.S03E115.DVDRip.XviD-OSiTV" },
                };

            Subject.GetRssDecision(_reports).Should().HaveCount(1);

            ExceptionVerification.ExpectedErrors(1);
        }

        [Test]
        public void should_return_unknown_series_rejection_if_series_title_is_an_alias_for_another_series()
        {
            GivenSpecifications(_pass1, _pass2, _pass3);

            Mocker.GetMock<ISceneMappingService>()
                  .Setup(s => s.FindIgdbId(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                  .Returns(12345);

            _remoteRom.Game = null;

            var result = Subject.GetRssDecision(_reports);

            result.Should().HaveCount(1);
            result.First().Rejections.First().Message.Should().Contain("12345");
        }
    }
}
