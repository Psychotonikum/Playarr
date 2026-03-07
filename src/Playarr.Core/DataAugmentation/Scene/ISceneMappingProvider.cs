using System.Collections.Generic;

namespace Playarr.Core.DataAugmentation.Scene
{
    public interface ISceneMappingProvider
    {
        List<SceneMapping> GetSceneMappings();
    }
}
