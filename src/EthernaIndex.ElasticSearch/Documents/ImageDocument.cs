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

using System.Collections.Generic;

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class ImageDocument
    {
        // Constructors.
        public ImageDocument(
            float aspectRatio,
            string blurhash,
            IEnumerable<SourceImageDocument> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public ImageDocument() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public float AspectRatio { get; set; }
        public string Blurhash { get; set; }
        public IEnumerable<SourceImageDocument> Sources { get; set; }
    }
}
