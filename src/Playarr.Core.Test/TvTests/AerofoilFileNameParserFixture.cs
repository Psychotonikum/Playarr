using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Games;

namespace Playarr.Core.Test.TvTests
{
    [TestFixture]
    public class AerofoilFileNameParserFixture
    {
        // Classic single-file with region
        [TestCase("Super Mario World USA.sfc", "Super Mario World", "USA", RomFileType.Base, null, null)]
        [TestCase("Chrono Trigger EUR.sfc", "Chrono Trigger", "EUR", RomFileType.Base, null, null)]
        [TestCase("Secret of Mana JPN.sfc", "Secret of Mana", "JPN", RomFileType.Base, null, null)]
        [TestCase("Pokemon Red World.gb", "Pokemon Red", "World", RomFileType.Base, null, null)]
        public void should_parse_classic_with_region(
            string fileName,
            string expectedTitle,
            string expectedRegion,
            RomFileType expectedType,
            string expectedVersion,
            string expectedDlcIndex)
        {
            var result = AerofoilFileNameParser.Parse(fileName);

            result.Should().NotBeNull();
            result.GameTitle.Should().Be(expectedTitle);
            result.Region.Should().Be(expectedRegion);
            result.FileType.Should().Be(expectedType);
            result.Version.Should().Be(expectedVersion);
            result.DlcIndex.Should().Be(expectedDlcIndex);
        }

        // Base game without region
        [TestCase("The Legend of Zelda Tears of the Kingdom.nsp", "The Legend of Zelda Tears of the Kingdom", RomFileType.Base)]
        [TestCase("Super Smash Bros Ultimate.nsp", "Super Smash Bros Ultimate", RomFileType.Base)]
        public void should_parse_base_game(string fileName, string expectedTitle, RomFileType expectedType)
        {
            var result = AerofoilFileNameParser.Parse(fileName);

            result.Should().NotBeNull();
            result.GameTitle.Should().Be(expectedTitle);
            result.FileType.Should().Be(expectedType);
            result.Region.Should().BeNull();
        }

        // Update files
        [TestCase("The Legend of Zelda Tears of the Kingdom v2323231.nsp", "The Legend of Zelda Tears of the Kingdom", "2323231")]
        [TestCase("Super Smash Bros Ultimate v131072.nsp", "Super Smash Bros Ultimate", "131072")]
        [TestCase("Mario Kart 8 Deluxe v3932160.nsp", "Mario Kart 8 Deluxe", "3932160")]
        public void should_parse_update(string fileName, string expectedTitle, string expectedVersion)
        {
            var result = AerofoilFileNameParser.Parse(fileName);

            result.Should().NotBeNull();
            result.GameTitle.Should().Be(expectedTitle);
            result.FileType.Should().Be(RomFileType.Update);
            result.Version.Should().Be(expectedVersion);
        }

        // DLC files
        [TestCase("The Legend of Zelda Tears of the Kingdom DLC01.nsp", "The Legend of Zelda Tears of the Kingdom", "01")]
        [TestCase("Super Smash Bros Ultimate DLC12.nsp", "Super Smash Bros Ultimate", "12")]
        [TestCase("Mario Kart 8 Deluxe DLC02.nsp", "Mario Kart 8 Deluxe", "02")]
        public void should_parse_dlc(string fileName, string expectedTitle, string expectedDlcIndex)
        {
            var result = AerofoilFileNameParser.Parse(fileName);

            result.Should().NotBeNull();
            result.GameTitle.Should().Be(expectedTitle);
            result.FileType.Should().Be(RomFileType.Dlc);
            result.DlcIndex.Should().Be(expectedDlcIndex);
        }

        // Extension extraction
        [TestCase("Super Mario World USA.sfc", "sfc")]
        [TestCase("The Legend of Zelda Tears of the Kingdom.nsp", "nsp")]
        [TestCase("Final Fantasy VII.iso", "iso")]
        public void should_extract_extension(string fileName, string expectedExt)
        {
            var result = AerofoilFileNameParser.Parse(fileName);

            result.Should().NotBeNull();
            result.Extension.Should().Be(expectedExt);
        }

        // Null and empty inputs
        [TestCase(null)]
        [TestCase("")]
        [TestCase("   ")]
        public void should_return_null_for_invalid_input(string fileName)
        {
            var result = AerofoilFileNameParser.Parse(fileName);

            result.Should().BeNull();
        }

        // ExtractGameTitle
        [TestCase("The Legend of Zelda Tears of the Kingdom v2323231.nsp", "The Legend of Zelda Tears of the Kingdom")]
        [TestCase("Super Smash Bros Ultimate DLC01.nsp", "Super Smash Bros Ultimate")]
        [TestCase("Chrono Trigger EUR.sfc", "Chrono Trigger")]
        [TestCase("Super Mario World.sfc", "Super Mario")]
        public void should_extract_game_title(string fileName, string expectedTitle)
        {
            var title = AerofoilFileNameParser.ExtractGameTitle(fileName);

            title.Should().Be(expectedTitle);
        }

        // Update priority: multiple updates should share same title for grouping
        [Test]
        public void should_group_updates_by_title()
        {
            var base1 = AerofoilFileNameParser.ExtractGameTitle("Zelda TOTK.nsp");
            var update1 = AerofoilFileNameParser.ExtractGameTitle("Zelda TOTK v100.nsp");
            var update2 = AerofoilFileNameParser.ExtractGameTitle("Zelda TOTK v200.nsp");
            var dlc1 = AerofoilFileNameParser.ExtractGameTitle("Zelda TOTK DLC01.nsp");

            base1.Should().Be("Zelda TOTK");
            update1.Should().Be("Zelda TOTK");
            update2.Should().Be("Zelda TOTK");
            dlc1.Should().Be("Zelda TOTK");
        }
    }
}
