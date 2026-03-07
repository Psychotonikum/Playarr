using FluentValidation;
using FluentValidation.Results;
using Playarr.Common.Extensions;
using Playarr.Core.Download.Clients.RTorrent;
using Playarr.Core.Validation.Paths;

namespace Playarr.Core.Download.Clients.rTorrent
{
    public interface IRTorrentDirectoryValidator
    {
        ValidationResult Validate(RTorrentSettings instance);
    }

    public class RTorrentDirectoryValidator : AbstractValidator<RTorrentSettings>, IRTorrentDirectoryValidator
    {
        public RTorrentDirectoryValidator(RootFolderValidator rootFolderValidator,
                                          PathExistsValidator pathExistsValidator,
                                          MappedNetworkDriveValidator mappedNetworkDriveValidator)
        {
            RuleFor(c => c.GameDirectory).Cascade(CascadeMode.Stop)
                .IsValidPath()
                                       .SetValidator(rootFolderValidator)
                                       .SetValidator(mappedNetworkDriveValidator)
                                       .SetValidator(pathExistsValidator)
                                       .When(c => c.GameDirectory.IsNotNullOrWhiteSpace())
                                       .When(c => c.Host == "localhost" || c.Host == "127.0.0.1");
        }
    }
}
