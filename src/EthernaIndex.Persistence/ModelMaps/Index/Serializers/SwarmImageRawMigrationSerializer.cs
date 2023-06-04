using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index.Serializers
{
    internal sealed class SwarmImageRawMigrationSerializer : IBsonSerializer<SwarmImageRaw>
    {
        public SwarmImageRawMigrationSerializer() { }

        public Type ValueType => throw new NotImplementedException();

        public SwarmImageRaw Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotImplementedException();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, SwarmImageRaw value)
        {
            // Don't need 
            throw new NotImplementedException();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            // Don't need 
            throw new NotImplementedException();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonSerializer.Deserialize(context.Reader, typeof(BsonDocument)) as BsonDocument ?? throw new NotImplementedException();
            BsonElement element;

            //aspect ratio
            bsonDocument.TryGetElement("AspectRatio", out element);
            var aspectRatio = element.Value.ToDouble();

            //blurhash
            bsonDocument.TryGetElement("BlurHash", out element);
            if (element.Value is null)
            {
                bsonDocument.TryGetElement("Blurhash", out element);
            }
            var blurhash = element.Value.ToString();

            //image source
            bsonDocument.TryGetElement("Sources", out element);
            var images = JsonSerializer.Deserialize<Dictionary<string, string>>(element.Value.ToJson())!;

            //create SwarmImageRaw
            return new SwarmImageRaw((float)aspectRatio,
                                     blurhash ?? "",
                                     images.Select(i => new ImageSource(Convert.ToInt32(i.Key.Replace("w",
                                                                                                      "",
                                                                                                      StringComparison.InvariantCultureIgnoreCase),
                                                                                        CultureInfo.InvariantCulture),
                                                                        null,
                                                                        i.Value)));
        }
    }
}
