using System;
using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Releases;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]
    public class AirDateSpecificationFixture : CoreTest<AirDateSpecification>
    {
        private RemoteRom _remoteRom;

        [SetUp]
        public void Setup()
        {
            _remoteRom = new RemoteRom
                           {
                               Game = new Game
                                        {
                                            Tags = new HashSet<int>()
                                        },
                               Roms = Builder<Rom>.CreateListOfSize(1)
                                   .All()
                                   .With(e => e.AirDateUtc = DateTime.UtcNow)
                                   .Build()
                                   .ToList(),
                               Release = new ReleaseInfo
                               {
                                   PublishDate = DateTime.UtcNow.AddDays(-1)
                               }
                           };
        }

        private void GivenSettings(bool airDateRestriction, int gracePeriod)
        {
            Mocker.GetMock<IReleaseProfileService>()
                .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                .Returns(new List<ReleaseProfile>
                {
                    new()
                    {
                        AirDateRestriction = airDateRestriction,
                        AirDateGracePeriod = gracePeriod
                    }
                });
        }

        [Test]
        public void should_be_true_if_profile_does_not_enforce_air_date_restriction()
        {
            GivenSettings(false, 0);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_release_date_is_after_air_date()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(1);

            GivenSettings(true, 0);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_release_date_with_grace_period_is_after_air_date()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(1);

            GivenSettings(true, -2);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_true_if_release_date_is_the_same_as_air_date()
        {
            var airDate = DateTime.UtcNow;
            _remoteRom.Roms.First().AirDateUtc = airDate;
            _remoteRom.Release.PublishDate = airDate;

            GivenSettings(true, 0);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_be_false_if_air_date_is_null()
        {
            _remoteRom.Roms.First().AirDateUtc = null;

            GivenSettings(true, -2);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_if_release_date_is_before_air_date()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(-1);

            GivenSettings(true, 0);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_if_release_date_with_grace_period_is_before_air_date()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(-3);

            GivenSettings(true, -2);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_if_release_date_is_after_air_date_and_grace_period_is_positive()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(1);

            GivenSettings(true, 2);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_if_release_date_with_highest_grace_period_is_before_air_date()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(-1);

            Mocker.GetMock<IReleaseProfileService>()
                .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                .Returns(new List<ReleaseProfile>
                {
                    new()
                    {
                        AirDateRestriction = true,
                        AirDateGracePeriod = 0
                    },
                    new()
                    {
                        AirDateRestriction = true,
                        AirDateGracePeriod = -5
                    }
                });

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_be_false_if_one_release_profile_does_not_allow_grabbing_before_air_date()
        {
            _remoteRom.Roms.First().AirDateUtc = DateTime.UtcNow;
            _remoteRom.Release.PublishDate = DateTime.UtcNow.AddDays(-1);

            Mocker.GetMock<IReleaseProfileService>()
                .Setup(s => s.EnabledForTags(It.IsAny<HashSet<int>>(), It.IsAny<int>()))
                .Returns(new List<ReleaseProfile>
                {
                    new()
                    {
                        AirDateRestriction = true,
                        AirDateGracePeriod = 0
                    },
                    new()
                    {
                        AirDateRestriction = false,
                        AirDateGracePeriod = 0
                    }
                });

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }
    }
}
