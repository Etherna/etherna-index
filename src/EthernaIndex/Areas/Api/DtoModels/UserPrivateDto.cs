using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class UserPrivateDto
    {
        // Constructors.
        public UserPrivateDto(
            string address,
            string? identityManifest,
            IEnumerable<string> prevAddresses)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            IdentityManifest = identityManifest;
            PrevAddresses = prevAddresses ?? throw new ArgumentNullException(nameof(prevAddresses));
        }

        // Properties.
        public string Address { get; }
        public string? IdentityManifest { get; }
        public IEnumerable<string> PrevAddresses { get; }
    }
}
