using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels.ManifestV2
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class VideoManifestDetailV2Dto
    {
        // Properties.
        //from v2.0
        public float AspectRatio { get; set; }
        public string BatchId { get; set; }
        public string Description { get; set; }
        public string? PersonalData { get; set; }
        public IEnumerable<VideoSourceV2Dto> Sources { get; set; }

        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
