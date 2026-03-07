using System.Reflection;
using FluentAssertions;
using NUnit.Framework;
using Playarr.Common.Reflection;
using Playarr.Core.Datastore;
using Playarr.Test.Common;

namespace Playarr.Common.Test.ReflectionTests
{
    public class ReflectionExtensionFixture : TestBase
    {
        [Test]
        public void should_get_properties_from_models()
        {
            var models = Assembly.Load("Playarr.Core").ImplementationsOf<ModelBase>();

            foreach (var model in models)
            {
                model.GetSimpleProperties().Should().NotBeEmpty();
            }
        }

        [Test]
        public void should_be_able_to_get_implementations()
        {
            var models = Assembly.Load("Playarr.Core").ImplementationsOf<ModelBase>();

            models.Should().NotBeEmpty();
        }
    }
}
