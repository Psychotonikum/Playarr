using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.CustomFormats;
using Playarr.Core.Languages;
using Playarr.Core.Parser.Model;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.CustomFormats.Specifications.LanguageSpecification
{
    [TestFixture]
    public class OriginalLanguageFixture : CoreTest<Core.CustomFormats.LanguageSpecification>
    {
        private CustomFormatInput _input;

        [SetUp]
        public void Setup()
        {
            _input = new CustomFormatInput
            {
                RomInfo = Builder<ParsedRomInfo>.CreateNew().Build(),
                Game = Builder<Game>.CreateNew().With(s => s.OriginalLanguage = Language.English).Build(),
                Size = 100.Megabytes(),
                Languages = new List<Language>
                {
                    Language.French
                },
                Filename = "Game.Title.S01E01"
            };
        }

        public void GivenLanguages(params Language[] languages)
        {
            _input.Languages = languages.ToList();
        }

        [Test]
        public void should_match_same_single_language()
        {
            GivenLanguages(Language.English);

            Subject.Value = Language.Original.Id;
            Subject.Negate = false;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_different_single_language()
        {
            Subject.Value = Language.Original.Id;
            Subject.Negate = false;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_not_match_negated_same_single_language()
        {
            GivenLanguages(Language.English);

            Subject.Value = Language.Original.Id;
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_match_negated_different_single_language()
        {
            Subject.Value = Language.Original.Id;
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }
    }
}
