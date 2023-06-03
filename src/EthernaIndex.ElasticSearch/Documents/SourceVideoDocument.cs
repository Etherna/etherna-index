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

namespace Etherna.EthernaIndex.ElasticSearch.Documents
{
    public class SourceVideoDocument
    {
        // Constructors.
        public SourceVideoDocument(
            string quality,
            string? path,
            string? reference,
            long size,
            string type)
        {
            Quality = quality;
            Path = path;
            Reference = reference;
            Size = size;
            Type = type;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public SourceVideoDocument() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public string Quality { get; set; }
        public string? Path { get; set; }
        public string? Reference { get; set; }
        public long Size { get; set; }
        public string Type { get; set; }
    }
}
