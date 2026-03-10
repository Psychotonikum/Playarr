using System.Collections.Generic;
using System.Linq;
using FizzWare.NBuilder;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Extensions;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class GameTitleSlugValidatorFixture : CoreTest<GameTitleSlugValidator>
    {
        private List<Game> _series;
        private TestValidator<Game> _validator;

        [SetUp]
        public void Setup()
        {
            _series = Builder<Game>.CreateListOfSize(1)
                                     .Build()
                                     .ToList();

            _validator = new TestValidator<Game>
                            {
                                v => v.RuleFor(s => s.TitleSlug).SetValidator(Subject)
                            };

            Mocker.GetMock<IGameService>()
                  .Setup(s => s.GetAllGames())
                  .Returns(_series);
        }

        [Test]
        public void should_not_be_valid_if_there_is_an_existing_series_with_the_same_title_slug()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.Id = 100)
                                        .With(s => s.TitleSlug = _series.First().TitleSlug)
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeFalse();
        }

        [Test]
        public void should_be_valid_if_there_is_not_an_existing_series_with_the_same_title_slug()
        {
            var game = Builder<Game>.CreateNew()
                                        .With(s => s.TitleSlug = "MyTitleSlug")
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_if_there_is_an_existing_series_with_a_null_title_slug()
        {
            _series.First().TitleSlug = null;

            var game = Builder<Game>.CreateNew()
                                        .With(s => s.TitleSlug = "MyTitleSlug")
                                        .Build();

            _validator.Validate(game).IsValid.Should().BeTrue();
        }

        [Test]
        public void should_be_valid_when_updating_an_existing_series()
        {
            _validator.Validate(_series.First().JsonClone()).IsValid.Should().BeTrue();
        }
    }
}
