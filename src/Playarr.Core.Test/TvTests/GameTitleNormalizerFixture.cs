using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class GameTitleNormalizerFixture
    {
        [TestCase("A to Z", 281588, "a to z")]
        public void should_use_precomputed_title(string title, int igdbId, string expected)
        {
            GameTitleNormalizer.Normalize(title, igdbId).Should().Be(expected);
        }

        [TestCase("2 Broke Girls", "2 broke girls")]
        [TestCase("Archer (2009)", "archer 2009")]
        [TestCase("The Office (US)", "office us")]
        [TestCase("The Mentalist", "mentalist")]
        [TestCase("The Good Wife", "good wife")]
        [TestCase("The Newsroom (2012)", "newsroom 2012")]
        [TestCase("Special Agent Oso", "special agent oso")]
        [TestCase("A.N.T. Farm", "ant farm")]
        [TestCase("A.I.C.O. -Incarnation-", "aico incarnation")]
        [TestCase("A.D. The Bible Continues", "ad the bible continues")]
        [TestCase("A.P. Bio", "ap bio")]
        [TestCase("A-Team", "ateam")]
        [TestCase("The A-Team", "ateam")]
        [TestCase("And Just Like That", "and just like that")]
        public void should_normalize_title(string title, string expected)
        {
            GameTitleNormalizer.Normalize(title, 0).Should().Be(expected);
        }
    }
}
