using System;

namespace Playarr.Common.Expansive
{
    public class CircularReferenceException : Exception
    {
        public CircularReferenceException(string message)
            : base(message)
        {
        }
    }
}
