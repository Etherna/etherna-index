using Digicando.DomainHelper.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Channel : EntityModelBase<string>
    {
        // Fields.
        private List<Video> _videos = new List<Video>();

        // Constructors.
        public Channel(string address, string bannerHash)
        {
            Address = address ?? throw new ArgumentNullException(nameof(address));
            SetBannerHash(bannerHash);
        }

        // Properties.
        public virtual string Address { get; protected set; }
        public virtual string BannerHash { get; protected set; }
        public virtual IEnumerable<Video> Videos
        {
            get => _videos;
            protected set => _videos = new List<Video>(value ?? new Video[0]);
        }

        // Methods.
        [PropertyAlterer(nameof(Videos))]
        public virtual void AddVideo(Video video)
        {
            if (!_videos.Contains(video))
                _videos.Add(video);
            video.SetOwnerChannel(this);
        }

        [PropertyAlterer(nameof(Videos))]
        public virtual void RemoveVideo(Video video)
        {
            _videos.Remove(video);
        }

        [PropertyAlterer(nameof(BannerHash))]
        public virtual void SetBannerHash(string hash) =>
            BannerHash = hash;
    }
}
