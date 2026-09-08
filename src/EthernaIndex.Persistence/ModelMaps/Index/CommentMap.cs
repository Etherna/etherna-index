// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.EthernaIndex.Domain.Models;
using Etherna.MongoDB.Bson.Serialization.Options;
using Etherna.MongoDB.Bson.Serialization.Serializers;
using Etherna.Scrinium.Core;
using Etherna.Scrinium.Core.Extensions;
using Etherna.Scrinium.Core.Options;
using Etherna.Scrinium.Core.Serialization;
using Etherna.Scrinium.Core.Serialization.Serializers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Index
{
    internal sealed class CommentMap : IModelMapsCollector
    {
        public void Register(IDbContextEngine dbContextEngine)
        {
            dbContextEngine.MapRegistry.AddModelMap<Comment>(
                "a846e95a-f99b-4d66-91a8-807a1ef34140", //v0.3.9
                mm =>
                {
                    mm.AutoMap();

                    // Set members with custom serializers.
                    mm.SetMemberSerializer(c => c.Author, UserMap.InformationSerializer(dbContextEngine));
                    mm.SetMemberSerializer(m => m.TextHistory, new ReadOnlyDictionarySerializer<DateTime, string>(
                        DictionaryRepresentation.ArrayOfDocuments, 
                        new DateTimeSerializer(),
                        new StringSerializer()));
                    mm.SetMemberSerializer(c => c.Video, VideoMap.ReferenceSerializer(dbContextEngine, OriginDeleteMode.DeleteReferencingDocument));
                })
                .AddSecondarySchema("8e509e8e-5c2b-4874-a734-ada4e2b91f92", //dev (pre v0.3.0), published for WAM event
                    mm =>
                    {
                        mm.AutoMap();

                        // Set members with custom serializers.
                        mm.SetMemberSerializer(c => c.Author, UserMap.InformationSerializer(dbContextEngine));
                        mm.SetMemberSerializer(c => c.Video, VideoMap.ReferenceSerializer(dbContextEngine, OriginDeleteMode.DeleteReferencingDocument));
                    }, fixDeserializedModelFunc: (_, comment) =>
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
