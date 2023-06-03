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

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class SourceDto
    {
        // Constructors.
        public SourceDto(
            string quality,
            string? path,
            string? reference,
            long size)
        {
            Quality = quality;
            Path = path;
            Reference = reference;
            Size = size;
        }

        // Properties.
        public string Quality { get; private set; }
        public string? Path { get; private set; }
        public string? Reference { get; private set; }
        public long Size { get; private set; }
    }
}
