using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Extensions;

namespace Playarr.Common.Test.ExtensionTests.StringExtensionTests
{
    [TestFixture]
    public class ReverseFixture
    {
        [TestCase("input", "tupni")]
        [TestCase("racecar", "racecar")]
        public void should_reverse_string(string input, string expected)
        {
            input.Reverse().Should().Be(expected);
        }
    }
}
