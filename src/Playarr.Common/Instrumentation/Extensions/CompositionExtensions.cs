using DryIoc;
using NLog;

namespace Playarr.Common.Instrumentation.Extensions
{
    public static class CompositionExtensions
    {
        public static IContainer AddPlayarrLogger(this IContainer container)
        {
            container.Register(Made.Of<Logger>(() => LogManager.GetLogger(Arg.Index<string>(0)), r => r.Parent.ImplementationType.Name.ToString()), reuse: Reuse.Transient);
            return container;
        }
    }
}
