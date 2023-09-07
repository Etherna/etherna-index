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
using Etherna.MongoDB.Bson.Serialization.Options;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Extensions;
using Etherna.MongODM.Core.Serialization;
using Etherna.MongODM.Core.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class CommentMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.MapRegistry.AddModelMap<Comment>(
                "a846e95a-f99b-4d66-91a8-807a1ef34140", //v0.3.9
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(c => c.Author, UserMap.InformationSerializer(dbContext));
                    mm.SetMemberSerializer(m => m.TextHistory, new ReadOnlyDictionarySerializer<DateTime, string>(
                        DictionaryRepresentation.ArrayOfDocuments, 
                        new DateTimeSerializer(),
                        new StringSerializer()));
                    mm.SetMemberSerializer(c => c.Video, VideoMap.ReferenceSerializer(dbContext));
                })
                .AddSecondarySchema("8e509e8e-5c2b-4874-a734-ada4e2b91f92", //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with custom serializers.
                        mm.SetMemberSerializer(c => c.Author, UserMap.InformationSerializer(dbContext));
                        mm.SetMemberSerializer(c => c.Video, VideoMap.ReferenceSerializer(dbContext));
                    }, fixDeserializedModelFunc: comment =>
                    {
                        var textHistory = new Dictionary<DateTime, string>
                        {
                            [comment.CreationDateTime] = (string)comment.ExtraElements!["Text"]
                        };

                        ReflectionHelper.SetValue(
                            comment,
                            v => v.TextHistory,
                            textHistory);
                        
                        return Task.FromResult(comment);
                    });
        }
    }
}
