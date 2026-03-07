using FluentValidation;
using Playarr.Core.ImportLists;
using Playarr.Core.Validation;
using Playarr.Core.Validation.Paths;
using Playarr.SignalR;
using Playarr.Http;

namespace Playarr.Api.V3.ImportLists
{
    [V3ApiController]
    public class ImportListController : ProviderControllerBase<ImportListResource, ImportListBulkResource, IImportList, ImportListDefinition>
    {
        public static readonly ImportListResourceMapper ResourceMapper = new();
        public static readonly ImportListBulkResourceMapper BulkResourceMapper = new();

        public ImportListController(IBroadcastSignalRMessage signalRBroadcaster,
            IImportListFactory importListFactory,
            RootFolderExistsValidator rootFolderExistsValidator,
            QualityProfileExistsValidator qualityProfileExistsValidator)
            : base(signalRBroadcaster, importListFactory, "importlist", ResourceMapper, BulkResourceMapper)
        {
            SharedValidator.RuleFor(c => c.RootFolderPath).Cascade(CascadeMode.Stop)
                .IsValidPath()
                .SetValidator(rootFolderExistsValidator);

            SharedValidator.RuleFor(c => c.QualityProfileId).Cascade(CascadeMode.Stop)
                .ValidId()
                .SetValidator(qualityProfileExistsValidator);
        }
    }
}
