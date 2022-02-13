using Etherna.EthernaIndex.Swarm.DtoModel;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoManifestDto
    {
        // Constructors.
        public VideoManifestDto(
            string hash,
            string? feedTopic,
            string? title,
            string? description,
            string? originalQuality,
            int? duration,
            ImageDto? thumbnail,
            IEnumerable<SourceDto>? sources)
        {
            Hash = hash;
            FeedTopic = feedTopic;
            Title = title;
            Description = description;
            OriginalQuality = originalQuality;
            Duration = duration;
            Thumbnail = thumbnail;
            Sources = sources;
        }

        // Properties.
        public string Hash { get; set; }
        public string? FeedTopic { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? OriginalQuality { get; set; }
        public int? Duration { get; set; }
        public ImageDto? Thumbnail { get; set; }
        public IEnumerable<SourceDto>? Sources { get; set; }
    }
}
