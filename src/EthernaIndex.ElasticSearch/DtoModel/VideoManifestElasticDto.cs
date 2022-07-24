using Nest;
using System;

namespace Etherna.EthernaIndex.ElasticSearch.DtoModel
{
    public class VideoManifestElasticDto
    {
        // Constructors.
        public VideoManifestElasticDto(
            string videoId,
            DateTime creationDateTime,
            string description,
            string title)
        {
            VideoId = videoId;
            CreationDateTime = creationDateTime;
            Description = description;
            Title = title;
        }

        // Properties.
        public string VideoId { get; init; }
        public DateTime CreationDateTime { get; init; }
        public string Description { get; init; }
        public string Title { get; init; }
    }
}
