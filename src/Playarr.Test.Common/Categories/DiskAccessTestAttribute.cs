using NUnit.Framework;

namespace Playarr.Test.Common.Categories
{
    public class DiskAccessTestAttribute : CategoryAttribute
    {
        public DiskAccessTestAttribute()
            : base("DiskAccessTest")
        {
        }
    }
}
