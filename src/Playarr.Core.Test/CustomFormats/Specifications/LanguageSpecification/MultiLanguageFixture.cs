using System.Collections.Generic;
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
    public class MultiLanguageFixture : CoreTest<Core.CustomFormats.LanguageSpecification>
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
                    Language.English,
                    Language.French
                },
                Filename = "Game.Title.S01E01"
            };
        }

        [Test]
        public void should_match_one_language()
        {
            Subject.Value = Language.French.Id;
            Subject.Negate = false;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_match_language_if_other_languages_are_present()
        {
            Subject.Value = Language.French.Id;
            Subject.ExceptLanguage = true;
            Subject.Negate = false;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_match_language_if_not_original_language_is_present()
        {
            Subject.Value = Language.Original.Id;
            Subject.ExceptLanguage = true;
            Subject.Negate = false;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_different_language()
        {
            Subject.Value = Language.Spanish.Id;
            Subject.Negate = false;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_not_match_negated_when_one_language_matches()
        {
            Subject.Value = Language.French.Id;
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }

        [Test]
        public void should_not_match_negated_when_all_languages_do_not_match()
        {
            Subject.Value = Language.Spanish.Id;
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeTrue();
        }

        [Test]
        public void should_not_match_negate_language_if_other_languages_are_present()
        {
            Subject.Value = Language.Spanish.Id;
            Subject.ExceptLanguage = true;
            Subject.Negate = true;

            Subject.IsSatisfiedBy(_input).Should().BeFalse();
        }
    }
}
