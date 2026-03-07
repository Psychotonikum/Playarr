using Playarr.Common.Exceptions;

namespace Playarr.Core.Update
{
    public class UpdateFailedException : PlayarrException
    {
        public UpdateFailedException(string message, params object[] args)
            : base(message, args)
        {
        }

        public UpdateFailedException(string message)
            : base(message)
        {
        }
    }
}
