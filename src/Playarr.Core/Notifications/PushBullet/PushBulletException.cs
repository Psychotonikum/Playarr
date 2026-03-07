using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Notifications.PushBullet
{
    public class PushBulletException : PlayarrException
    {
        public PushBulletException(string message)
            : base(message)
        {
        }

        public PushBulletException(string message, Exception innerException, params object[] args)
            : base(message, innerException, args)
        {
        }
    }
}
