using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MetadataVideoSchema1
    {
        // Properties.
        public string Description { get; set; }
        public long Duration { get; set; }
        public long CreatedAt { get; set; }
        public string OriginalQuality { get; set; }
        public string OwnerAddress { get; set; }
        public IEnumerable<MetadataVideoSourceSchema1> Sources { get; set; }
        public SwarmImageRawSchema1? Thumbnail { get; set; }
        public string Title { get; set; }
        public string V { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
