using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.MediaFiles
{
    public class ScriptImportException : PlayarrException
    {
        public ScriptImportException(string message)
            : base(message)
        {
        }

        public ScriptImportException(string message, params object[] args)
            : base(message, args)
        {
        }

        public ScriptImportException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
