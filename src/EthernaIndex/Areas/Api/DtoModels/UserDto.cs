using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class UserDto
    {
        // Constructors.
        public UserDto(User user)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            Address = user.Address;
            CreationDateTime = user.CreationDateTime;
            IdentityManifest = user.IdentityManifest?.Hash;
        }

        // Properties.
        public string Address { get; }
        public DateTime CreationDateTime { get; }
        public string? IdentityManifest { get; }
    }
}
