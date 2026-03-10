using System;
using System.Collections.Generic;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.CustomFormats;
using Playarr.Core.DecisionEngine.Specifications;
using Playarr.Core.Parser.Model;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.CustomFormats;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.DecisionEngineTests
{
    [TestFixture]

    public class CustomFormatAllowedByProfileSpecificationFixture : CoreTest<CustomFormatAllowedbyProfileSpecification>
    {
        private RemoteRom _remoteRom;

        private CustomFormat _format1;
        private CustomFormat _format2;

        [SetUp]
        public void Setup()
        {
            _format1 = new CustomFormat("Awesome Format");
            _format1.Id = 1;

            _format2 = new CustomFormat("Cool Format");
            _format2.Id = 2;

            var fakeSeries = Builder<Game>.CreateNew()
                .With(c => c.QualityProfile = new QualityProfile
                {
                    Cutoff = Quality.Bluray1080p.Id,
                    MinFormatScore = 1
                })
                .Build();

            _remoteRom = new RemoteRom
            {
                Game = fakeSeries,
                ParsedRomInfo = new ParsedRomInfo { Quality = new QualityModel(Quality.DVD, new Revision(version: 2)) },
            };

            CustomFormatsTestHelpers.GivenCustomFormats(_format1, _format2);
        }

        [Test]
        public void should_allow_if_format_score_greater_than_min()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { _format1 };
            _remoteRom.Game.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(_format1.Name);
            _remoteRom.CustomFormatScore = _remoteRom.Game.QualityProfile.Value.CalculateCustomFormatScore(_remoteRom.CustomFormats);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_format_score_not_greater_than_min()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { _format2 };
            _remoteRom.Game.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(_format1.Name);
            _remoteRom.CustomFormatScore = _remoteRom.Game.QualityProfile.Value.CalculateCustomFormatScore(_remoteRom.CustomFormats);

            Console.WriteLine(_remoteRom.CustomFormatScore);
            Console.WriteLine(_remoteRom.Game.QualityProfile.Value.MinFormatScore);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_deny_if_format_score_not_greater_than_min_2()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { _format2, _format1 };
            _remoteRom.Game.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(_format1.Name);
            _remoteRom.CustomFormatScore = _remoteRom.Game.QualityProfile.Value.CalculateCustomFormatScore(_remoteRom.CustomFormats);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_all_format_is_defined_in_profile()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { _format2, _format1 };
            _remoteRom.Game.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteRom.CustomFormatScore = _remoteRom.Game.QualityProfile.Value.CalculateCustomFormatScore(_remoteRom.CustomFormats);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }

        [Test]
        public void should_deny_if_no_format_was_parsed_and_min_score_positive()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { };
            _remoteRom.Game.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteRom.CustomFormatScore = _remoteRom.Game.QualityProfile.Value.CalculateCustomFormatScore(_remoteRom.CustomFormats);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeFalse();
        }

        [Test]
        public void should_allow_if_no_format_was_parsed_min_score_is_zero()
        {
            _remoteRom.CustomFormats = new List<CustomFormat> { };
            _remoteRom.Game.QualityProfile.Value.FormatItems = CustomFormatsTestHelpers.GetSampleFormatItems(_format1.Name, _format2.Name);
            _remoteRom.Game.QualityProfile.Value.MinFormatScore = 0;
            _remoteRom.CustomFormatScore = _remoteRom.Game.QualityProfile.Value.CalculateCustomFormatScore(_remoteRom.CustomFormats);

            Subject.IsSatisfiedBy(_remoteRom, new()).Accepted.Should().BeTrue();
        }
    }
}
