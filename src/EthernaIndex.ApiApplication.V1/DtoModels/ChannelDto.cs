using Etherna.EthernaIndex.Domain.Models;
using System;

namespace Etherna.EthernaIndex.ApiApplication.V1.DtoModels
{
    public class ChannelDto
    {
        // Constructors.
        public ChannelDto(Channel channel)
        {
            Address = channel.Address;
            CreationDateTime = channel.CreationDateTime;
        }

        // Properties.
        public string Address { get; }
        public DateTime CreationDateTime { get; }
    }
}
