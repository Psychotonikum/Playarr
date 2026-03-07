using System;

namespace Playarr.Common.Messaging
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class LifecycleEventAttribute : Attribute
    {
    }
}
