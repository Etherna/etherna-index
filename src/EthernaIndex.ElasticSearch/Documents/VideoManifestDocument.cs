using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Nest;
using System;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class VideoManifestDocument
    {
        // Constructors.
        public VideoManifestDocument(
            Video video,
            VideoManifest videoManifest)
        {
            VideoId = video.Id;
            CreationDateTime = videoManifest.CreationDateTime;
            Description = videoManifest.Description ?? "";
            Title = videoManifest.Title ?? "";
        }

        // Properties.
        public string VideoId { get; init; }
        public DateTime CreationDateTime { get; init; }
        public string Description { get; init; }
        public string Title { get; init; }
    }
}
