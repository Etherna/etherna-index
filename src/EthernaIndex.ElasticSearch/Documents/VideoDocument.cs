using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using EthernaIndex.ElasticSearch.Documents;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class VideoDocument
    {
        // Constructors.
        public VideoDocument(
            Video video)
        {
            if (video.LastValidManifest is null)
            {
                var ex = new InvalidOperationException("Null valid manifest");
                ex.Data.Add("VideoId", video.Id);
                throw ex;
            }

            Id = video.Id;
            CreationDateTime = video.LastValidManifest.CreationDateTime;
            BatchId = video.LastValidManifest.BatchId;
            Description = video.LastValidManifest.Description ?? "";
            Duration = video.LastValidManifest.Duration;
            IsFrozen = video.IsFrozen;
            ManifestHash = video.LastValidManifest.Manifest.Hash;
            OriginalQuality = video.LastValidManifest.OriginalQuality;
            OwnerSharedInfoId = video.Owner.SharedInfoId;
            Sources = video.LastValidManifest.Sources.Select(i => new SourceDocument(i.Bitrate, i.Quality, i.Reference, i.Size));
            Title = video.LastValidManifest.Title ?? "";

            if (video.LastValidManifest.Thumbnail is not null)
                Thumbnail = new ImageDocument(
                    video.LastValidManifest.Thumbnail.AspectRatio,
                    video.LastValidManifest.Thumbnail.Blurhash,
                    video.LastValidManifest.Thumbnail.Sources);
        }

        // Properties.
        public string Id { get; }
        public DateTime CreationDateTime { get; }
        public string? BatchId { get; }
        public string Description { get; }
        public long? Duration { get; }
        public bool IsFrozen { get; }
        public string ManifestHash { get; }
        public string? OriginalQuality { get; }
        public string OwnerSharedInfoId { get; }
        public IEnumerable<SourceDocument> Sources { get; }
        public ImageDocument? Thumbnail { get; }
        public string Title { get; }
        public long TotDownvotes { get; }
        public long TotUpvotes { get; }
    }
}
