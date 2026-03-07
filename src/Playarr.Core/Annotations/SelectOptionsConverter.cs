using System.Collections.Generic;

namespace Playarr.Core.Annotations
{
    public interface ISelectOptionsConverter
    {
        List<SelectOption> GetSelectOptions();
    }
}
