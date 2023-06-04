using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Bson.Serialization;
using System;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index.Serializers
{
    internal sealed class VideoSourceMigrationSerializer : IBsonSerializer<VideoSource>
    {
        public Type ValueType => throw new NotImplementedException();

        public VideoSource Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            throw new NotImplementedException();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, VideoSource value)
        {
            throw new NotImplementedException();
        }

        public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
        {
            throw new NotImplementedException();
        }

        object IBsonSerializer.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonDocument = BsonSerializer.Deserialize(context.Reader, typeof(BsonDocument)) as BsonDocument ?? throw new NotImplementedException();
            BsonElement element;

            //quality
            bsonDocument.TryGetElement("Quality", out element);
            var quality = element.Value.ToString();

            //path
            bsonDocument.TryGetElement("Reference", out element);
            if (element.Value is null)
            {
                bsonDocument.TryGetElement("Path", out element);
            }
            var path = element.Value.ToString();

            //size
            bsonDocument.TryGetElement("Size", out element);
            var size = element.Value.ToInt64();

            //path
            bsonDocument.TryGetElement("Type", out element);
            var type = element.Value?.ToString();

            //create VideoSource
            return new VideoSource(quality ?? "",
                                   path ?? "",
                                   size,
                                   type);
        }
    }
}
