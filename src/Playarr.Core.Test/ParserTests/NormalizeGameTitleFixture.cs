using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Parser;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.ParserTests
{
    [TestFixture]
    public class NormalizeGameTitleFixture : CoreTest
    {
        [TestCase("Game", "game")]
        [TestCase("Game (2009)", "game2009")]
        [TestCase("Game.2010", "game2010")]
        [TestCase("Game_and_Title_Playarr", "gametitleplayarr")]
        public void should_normalize_series_title(string parsedGameName, string seriesName)
        {
            var result = parsedGameName.CleanGameTitle();
            result.Should().Be(seriesName);
        }

        [TestCase("CaPitAl", "capital")]
        [TestCase("peri.od", "period")]
        [TestCase("this.^&%^**$%@#$!That", "thisthat")]
        [TestCase("test/test", "testtest")]
        [TestCase("90210", "90210")]
        [TestCase("24", "24")]
        [TestCase("Test: Something à Deux", "testsomethingdeux")]
        [TestCase("Parler à", "parlera")]
        [TestCase("Ríkið", "rikid")]
        public void should_remove_special_characters_and_casing(string dirty, string clean)
        {
            var result = dirty.CleanGameTitle();
            result.Should().Be(clean);
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_remove_common_words_from_middle_of_title(string word)
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}.word",
                                "word {0} word",
                                "word-{0}-word"
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanGameTitle().Should().Be("wordword");
            }
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_not_remove_common_words_from_end_of_title(string word)
        {
            var dirtyFormat = new[]
                              {
                                  "word.word.{0}",
                                  "word-word-{0}",
                                  "word-word {0}"
                              };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanGameTitle().Should().Be("wordword" + word.ToLower());
            }
        }

        [Test]
        public void should_remove_a_from_middle_of_title()
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}.word",
                                "word {0} word",
                                "word-{0}-word",
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, "a");
                dirty.CleanGameTitle().Should().Be("wordword");
            }
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("a")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_not_remove_common_words_in_the_middle_of_word(string word)
        {
            var dirtyFormat = new[]
                            {
                                "word.{0}word",
                                "word {0}word",
                                "word-{0}word",
                                "word{0}.word",
                                "word{0}-word",
                                "word{0}-word",
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanGameTitle().Should().Be("word" + word.ToLower() + "word");
            }
        }

        [TestCase("The Game", "thegame")]
        [TestCase("The Game Show With Playarr Dev", "thegameshowwithplayarrdev")]
        [TestCase("The.Game.Show", "thegameshow")]
        public void should_not_remove_from_the_beginning_of_the_title(string parsedGameName, string seriesName)
        {
            var result = parsedGameName.CleanGameTitle();
            result.Should().Be(seriesName);
        }

        [TestCase("the")]
        [TestCase("and")]
        [TestCase("or")]
        [TestCase("a")]
        [TestCase("an")]
        [TestCase("of")]
        public void should_not_clean_word_from_beginning_of_string(string word)
        {
            var dirtyFormat = new[]
                            {
                                "{0}.word.word",
                                "{0}-word-word",
                                "{0} word word"
                            };

            foreach (var s in dirtyFormat)
            {
                var dirty = string.Format(s, word);
                dirty.CleanGameTitle().Should().Be(word + "wordword");
            }
        }

        [Test]
        public void should_not_clean_trailing_a()
        {
            "Game Title A".CleanGameTitle().Should().Be("gametitlea");
        }

        [TestCase("3%", "3percent")]
        [TestCase("Game Top & 100% Coding Developers", "gametop100percentcodingdevelopers")]
        [TestCase("Game Title What's Your F@%king Deal?!", "gametitlewhatsyourfkingdeal")]
        public void should_replace_percent_sign_with_percent_following_numbers(string input, string expected)
        {
            input.CleanGameTitle().Should().Be(expected);
        }
    }
}
