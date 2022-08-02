using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Etherna.EthernaIndex.Swarm.DtoModels
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public class MetadataVideoSourceSchema1
    {
        // Properties.
        public int Bitrate { get; set; }
        public string Quality { get; set; }
        public string Reference { get; set; }
        public long Size { get; set; }

#pragma warning disable CA2227 // Collection properties should be read only
        [JsonExtensionData]
        public Dictionary<string, JsonElement>? ExtraElements { get; set; }
#pragma warning restore CA2227 // Collection properties should be read only
    }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
}
