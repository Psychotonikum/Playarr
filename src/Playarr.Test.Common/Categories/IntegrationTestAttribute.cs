using NUnit.Framework;

namespace Playarr.Test.Common.Categories
{
    public class IntegrationTestAttribute : CategoryAttribute
    {
        public IntegrationTestAttribute()
            : base("IntegrationTest")
        {
        }
    }
}
