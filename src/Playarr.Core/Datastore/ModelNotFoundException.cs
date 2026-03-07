using System;
using Playarr.Common.Exceptions;

namespace Playarr.Core.Datastore
{
    public class ModelNotFoundException : PlayarrException
    {
        public ModelNotFoundException(Type modelType, int modelId)
            : base("{0} with ID {1} does not exist", modelType.Name, modelId)
        {
        }
    }
}
