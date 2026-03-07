using System;
using Microsoft.AspNetCore.Mvc;

namespace Playarr.Http.REST.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class RestDeleteByIdAttribute : HttpDeleteAttribute
    {
        public RestDeleteByIdAttribute()
            : base("{id:int}")
        {
        }
    }
}
