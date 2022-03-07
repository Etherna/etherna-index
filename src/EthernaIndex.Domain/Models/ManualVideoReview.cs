using System;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class ManualVideoReview : ManualReviewBase
    {
        // Constructors.
        public ManualVideoReview(
            User author,
            string description,
            bool isValid,
            Video video,
            VideoManifest videoManifest)
            : base(author, description, isValid)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            // Check manifest.
            if (!video.VideoManifests.Contains(videoManifest))
            {
                var ex = new InvalidOperationException("Missmatching between manifest and video");
                ex.Data.Add("Video.Id", video.Id);
                ex.Data.Add("VideoManifest.Hash", videoManifest.Manifest.Hash);
                ex.Data.Add("VideoManifest.Id", videoManifest.Id);
                throw ex;
            }

            ManifestHash = videoManifest.Manifest.Hash;
            VideoId = video.Id;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ManualVideoReview() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string ManifestHash { get; protected set; }
        public virtual string VideoId { get; private set; }
    }
}
