using System;
using System.Data;
using Dapper;
using Playarr.Common.Disk;

namespace Playarr.Core.Datastore.Converters
{
    public class OsPathConverter : SqlMapper.TypeHandler<OsPath>
    {
        public override void SetValue(IDbDataParameter parameter, OsPath value)
        {
            parameter.Value =  value.FullPath;
        }

        public override OsPath Parse(object value)
        {
            if (value == null || value is DBNull)
            {
                return new OsPath(null);
            }

            return new OsPath((string)value);
        }
    }
}
