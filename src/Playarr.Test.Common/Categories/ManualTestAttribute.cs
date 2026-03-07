using NUnit.Framework;

namespace Playarr.Test.Common.Categories
{
    public class ManualTestAttribute : CategoryAttribute
    {
        public ManualTestAttribute()
            : base("ManualTest")
        {
        }
    }
}
