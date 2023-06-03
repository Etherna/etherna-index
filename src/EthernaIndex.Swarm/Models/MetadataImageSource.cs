﻿//   Copyright 2021-present Etherna Sagl
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
    public class MetadataImageSource
    {
        // Constructors.
        public MetadataImageSource(
            int width,
            string? path,
            string? reference,
            string? type)
        {
            Width = width;
            Path = path;
            Reference = reference;
            Type = type;
        }
        internal MetadataImageSource(MetadataImageSourceSchema2 metadataImageSource) :
            this(metadataImageSource.Width,
                metadataImageSource.Path,
                metadataImageSource.Reference,
                metadataImageSource.Type)
        { }

        // Properties.
        public string? Path { get; set; }
        public string? Reference { get; set; }
        public string? Type { get; set; }
        public int Width { get; set; }
    }
}
