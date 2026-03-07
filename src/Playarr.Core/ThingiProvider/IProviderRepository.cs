using Playarr.Core.Datastore;

namespace Playarr.Core.ThingiProvider
{
    public interface IProviderRepository<TProvider> : IBasicRepository<TProvider>
        where TProvider : ModelBase, new()
    {
// void DeleteImplementations(string implementation);
    }
}
