using System;
using System.Collections.Generic;
using System.IO;
using FizzWare.NBuilder;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Moq;
using NUnit.Framework;
using Playarr.Core.Exceptions;
using Playarr.Core.MetadataSource;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;
using Playarr.Core.Games;
using Playarr.Test.Common;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class AddGameFixture : CoreTest<AddGameService>
    {
        private Game _fakeSeries;

        [SetUp]
        public void Setup()
        {
            _fakeSeries = Builder<Game>
                .CreateNew()
                .With(s => s.Path = null)
                .Build();
        }

        private void GivenValidSeries(int igdbId)
        {
            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(igdbId))
                  .Returns(new Tuple<Game, List<Rom>>(_fakeSeries, new List<Rom>()));
        }

        private void GivenValidPath()
        {
            Mocker.GetMock<IBuildFileNames>()
                  .Setup(s => s.GetGameFolder(It.IsAny<Game>(), null))
                  .Returns<Game, NamingConfig>((c, n) => c.Title);

            Mocker.GetMock<IAddGameValidator>()
                  .Setup(s => s.Validate(It.IsAny<Game>()))
                  .Returns(new ValidationResult());
        }

        [Test]
        public void should_be_able_to_add_a_series_without_passing_in_title()
        {
            var newGame = new Game
            {
                IgdbId = 1,
                RootFolderPath = @"C:\Test\TV"
            };

            GivenValidSeries(newGame.IgdbId);
            GivenValidPath();

            var game = Subject.AddGame(newGame);

            game.Title.Should().Be(_fakeSeries.Title);
        }

        [Test]
        public void should_have_proper_path()
        {
            var newGame = new Game
                            {
                                IgdbId = 1,
                                RootFolderPath = @"C:\Test\TV"
                            };

            GivenValidSeries(newGame.IgdbId);
            GivenValidPath();

            var game = Subject.AddGame(newGame);

            game.Path.Should().Be(Path.Combine(newGame.RootFolderPath, _fakeSeries.Title));
        }

        [Test]
        public void should_throw_if_series_validation_fails()
        {
            var newGame = new Game
            {
                IgdbId = 1,
                Path = @"C:\Test\TV\Title1"
            };

            GivenValidSeries(newGame.IgdbId);

            Mocker.GetMock<IAddGameValidator>()
                  .Setup(s => s.Validate(It.IsAny<Game>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddGame(newGame));
        }

        [Test]
        public void should_throw_if_series_cannot_be_found()
        {
            var newGame = new Game
            {
                IgdbId = 1,
                Path = @"C:\Test\TV\Title1"
            };

            Mocker.GetMock<IProvideSeriesInfo>()
                  .Setup(s => s.GetSeriesInfo(newGame.IgdbId))
                  .Throws(new SeriesNotFoundException(newGame.IgdbId));

            Mocker.GetMock<IAddGameValidator>()
                  .Setup(s => s.Validate(It.IsAny<Game>()))
                  .Returns(new ValidationResult(new List<ValidationFailure>
                                                {
                                                    new ValidationFailure("Path", "Test validation failure")
                                                }));

            Assert.Throws<ValidationException>(() => Subject.AddGame(newGame));

            ExceptionVerification.ExpectedErrors(1);
        }
    }
}
