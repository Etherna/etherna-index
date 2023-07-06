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

using Etherna.EthernaIndex.Swarm.DtoModels;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class MetadataVideoSource
    {
        // Constructors.
        public MetadataVideoSource(
            string quality,
            string path,
            long size,
            string type)
        {
            Quality = quality;
            Path = path;
            Size = size;
            Type = type;
        }
        internal MetadataVideoSource(MetadataVideoSourceSchema1 metadataVideoSource) :
            this(metadataVideoSource.Quality,
                metadataVideoSource.Reference,
                metadataVideoSource.Size,
                "mp4")
        { }
        internal MetadataVideoSource(MetadataVideoSourceSchema2 metadataVideoSource) :
            this(metadataVideoSource.Quality,
                metadataVideoSource.Path,
                metadataVideoSource.Size,
                metadataVideoSource.Type)
        { }

        // Properties.
        public string Path { get; }
        public string Quality { get; }
        public long Size { get; }
        public string Type { get; }
    }
}
