using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class ChannelDto
    {
        // Constructors.
        public ChannelDto(Channel channel)
        {
            if (channel is null)
                throw new ArgumentNullException(nameof(channel));

            Address = channel.Address;
            CreationDateTime = channel.CreationDateTime;
        }

        // Properties.
        public string Address { get; }
        public DateTime CreationDateTime { get; }
    }
}
