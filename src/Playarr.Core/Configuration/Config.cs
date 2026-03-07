using Playarr.Core.Datastore;

namespace Playarr.Core.Configuration
{
    public class Config : ModelBase
    {
        private string _key;

        public string Key
        {
            get { return _key; }
            set { _key = value.ToLowerInvariant(); }
        }

        public string Value { get; set; }
    }
}
