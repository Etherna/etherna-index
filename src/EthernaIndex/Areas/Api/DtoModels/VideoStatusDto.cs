using Etherna.EthernaIndex.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoStatusDto
    {
        // Constructors.
        public VideoStatusDto(Video video)
        {
            if (video is null)
                throw new ArgumentNullException(nameof(video));

            Id = video.Id;
            ManifestsStatus = video.VideoManifests.Select(vm => new VideoManifestStatusDto(vm));
        }

        // Properties.
        public string Id { get; private set; }
        public IEnumerable<VideoManifestStatusDto> ManifestsStatus { get; private set; }
    }
}
