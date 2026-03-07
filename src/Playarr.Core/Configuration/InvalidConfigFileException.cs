using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Configuration
{
    public class InvalidConfigFileException : PlayarrException
    {
        public InvalidConfigFileException(string message)
            : base(message)
        {
        }

        public InvalidConfigFileException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
