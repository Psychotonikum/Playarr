using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Core.Profiles.Qualities;
using Playarr.Core.Qualities;
using Playarr.Core.Test.Framework;

namespace Playarr.Core.Test.Qualities
{
    [TestFixture]
    public class QualityFixture : CoreTest
    {
        public static object[] FromIntCases =
                {
                        new object[] { 0, Quality.Unknown },
                        new object[] { 1, Quality.Bad },
                        new object[] { 2, Quality.Verified },
                };

        public static object[] ToIntCases =
                {
                        new object[] { Quality.Unknown, 0 },
                        new object[] { Quality.Bad, 1 },
                        new object[] { Quality.Verified, 2 },
                };

        [Test]
        [TestCaseSource(nameof(FromIntCases))]
        public void should_be_able_to_convert_int_to_qualityTypes(int source, Quality expected)
        {
            var quality = (Quality)source;
            quality.Should().Be(expected);
        }

        [Test]
        [TestCaseSource(nameof(ToIntCases))]
        public void should_be_able_to_convert_qualityTypes_to_int(Quality source, int expected)
        {
            var i = (int)source;
            i.Should().Be(expected);
        }

        [Test]
        public void all_qualities_should_contain_exactly_three()
        {
            Quality.All.Should().HaveCount(3);
            Quality.All.Should().Contain(Quality.Unknown);
            Quality.All.Should().Contain(Quality.Bad);
            Quality.All.Should().Contain(Quality.Verified);
        }

        [Test]
        public void video_quality_aliases_should_map_to_unknown()
        {
            Quality.SDTV.Should().Be(Quality.Unknown);
            Quality.DVD.Should().Be(Quality.Unknown);
            Quality.HDTV720p.Should().Be(Quality.Unknown);
            Quality.Bluray1080p.Should().Be(Quality.Unknown);
            Quality.WEBDL720p.Should().Be(Quality.Unknown);
        }

        public static List<QualityProfileQualityItem> GetDefaultQualities(params Quality[] allowed)
        {
            var qualities = new List<Quality>
            {
                Quality.Unknown,
                Quality.Bad,
                Quality.Verified,
            };

            if (allowed.Length == 0)
            {
                allowed = qualities.ToArray();
            }

            var items = qualities
                .Except(allowed)
                .Concat(allowed)
                .Select(v => new QualityProfileQualityItem { Quality = v, Allowed = allowed.Contains(v) }).ToList();

            return items;
        }
    }
}
