using Etherna.EthernaIndex.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.ApiApplication.V1.DtoModels
{
    public class ChannelDto
    {
        // Constructors.
        public ChannelDto(Channel channel)
        {
            Address = channel.Address;
            BannerHash = channel.BannerHash;
            CreationDateTime = channel.CreationDateTime;
            Videos = channel.Videos.Select(v => new VideoDto(v));
        }

        // Properties.
        public string Address { get; }
        public string BannerHash { get; }
        public DateTime CreationDateTime { get; }
        public IEnumerable<VideoDto> Videos { get; }
    }
}
