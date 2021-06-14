//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongODM;
using Etherna.MongODM.Extensions;
using Etherna.MongODM.Serialization;
using Etherna.MongODM.Serialization.Serializers;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Serializers;

namespace Etherna.EthernaIndex.Persistence.ModelMaps
{
    class UserMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.DocumentSchemaRegister.RegisterModelSchema<User>("0.1.0",
                cm =>
                {
                    cm.AutoMap();

                    // Set members with custom serializers.
                    cm.SetMemberSerializer(c => c.Videos,
                        new EnumerableSerializer<Video>(
                            VideoMap.InformationSerializer(dbContext, true)));
                });
        }

        /// <summary>
        /// The full entity serializer without relations
        /// </summary>
        public static ReferenceSerializer<User, string> InformationSerializer(
            IDbContext dbContext,
            bool useCascadeDelete = false) =>
            new ReferenceSerializer<User, string>(dbContext, useCascadeDelete)
                .RegisterType<ModelBase>()
                .RegisterType<EntityModelBase>(cm => { })
                .RegisterType<EntityModelBase<string>>(cm =>
                {
                    cm.MapIdMember(m => m.Id);
                    cm.IdMemberMap.SetSerializer(new StringSerializer(BsonType.ObjectId));
                })
                .RegisterType<User>(cm =>
                {
                    cm.MapMember(u => u.Address);
                    cm.MapMember(u => u.IdentityManifest);
                })
                .RegisterProxyType<User>();
    }
}
