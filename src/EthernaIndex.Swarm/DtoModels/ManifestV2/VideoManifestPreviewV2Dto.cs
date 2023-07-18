using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels.ManifestV2
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class VideoManifestPreviewV2Dto
    {
        // Properties.
        //from v2.0
        public long CreatedAt { get; set; }
        public long Duration { get; set; }
        public string OwnerAddress { get; set; }
        public ThumbnailV2Dto? Thumbnail { get; set; }
        public string Title { get; set; }
        public long? UpdatedAt { get; set; }
        public string V { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
