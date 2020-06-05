using Etherna.MongODM.Attributes;
using Nethereum.Util;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Channel : EntityModelBase<string>
    {
        // Fields.
        private List<Video> _videos = new List<Video>();

        // Constructors.
        public Channel(string address)
        {
            SetAddress(address);
        }
        protected Channel() { }

        // Properties.
        public virtual string Address { get; protected set; } = default!;
        public virtual IEnumerable<Video> Videos
        {
            get => _videos;
            protected set => _videos = new List<Video>(value ?? Array.Empty<Video>());
        }

        // Methods.
        [PropertyAlterer(nameof(Videos))]
        protected internal virtual void AddVideo(Video video)
        {
            if (!_videos.Contains(video))
                _videos.Add(video);
        }

        [PropertyAlterer(nameof(Videos))]
        protected internal virtual void RemoveVideo(Video video)
        {
            _videos.Remove(video);
        }

        // Helpers.
        private void SetAddress(string address)
        {
            if (address is null)
                throw new ArgumentNullException(nameof(address));
            if (!address.IsValidEthereumAddressHexFormat())
                throw new ArgumentException("The value is not a valid address", nameof(address));

            Address = address.ConvertToEthereumChecksumAddress();
        }
    }
}
