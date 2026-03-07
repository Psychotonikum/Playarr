using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Datastore
{
    public class ModelConflictException : PlayarrException
    {
        public ModelConflictException(Type modelType, int modelId)
            : base("{0} with ID {1} cannot be modified", modelType.Name, modelId)
        {
        }

        public ModelConflictException(Type modelType, int modelId, string message)
            : base("{0} with ID {1} {2}", modelType.Name, modelId, message)
        {
        }
    }
}
