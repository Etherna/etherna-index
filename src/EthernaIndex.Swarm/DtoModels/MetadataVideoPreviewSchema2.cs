using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MetadataVideoPreviewSchema2
    {
        // Properties.
        public long Duration { get; set; }
        public long CreatedAt { get; set; }
        public string OwnerAddress { get; set; }
        public SwarmImageRawSchema2? Thumbnail { get; set; }
        public string Title { get; set; }
        public long? UpdatedAt { get; set; } //added with v1.1
        public string V { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
