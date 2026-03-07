using System;
using Playarr.Common.TPL;

namespace Playarr.Test.Common
{
    public class MockDebouncer : Debouncer
    {
        public MockDebouncer(Action action, TimeSpan debounceDuration)
            : base(action, debounceDuration)
        {
        }

        public override void Execute()
        {
            lock (_timer)
            {
                _action();
            }
        }
    }
}
