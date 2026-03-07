using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Organizer;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.OrganizerTests
{
    [TestFixture]
    public class CleanFilenameFixture : CoreTest
    {
        [TestCase("Law & Order: Criminal Intent - S10E07 - Icarus [HDTV-720p]", "Law & Order - Criminal Intent - S10E07 - Icarus [HDTV-720p]")]
        public void should_replaace_invalid_characters(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(".hack s01e01", "hack s01e01")]
        public void should_remove_periods_from_start(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }

        [TestCase(" Game Title - S01E01 - Rom Title", "Game Title - S01E01 - Rom Title")]
        [TestCase("Game Title - S01E01 - Rom Title ", "Game Title - S01E01 - Rom Title")]
        public void should_remove_spaces_from_start_and_end(string name, string expectedName)
        {
            FileNameBuilder.CleanFileName(name).Should().Be(expectedName);
        }
    }
}
