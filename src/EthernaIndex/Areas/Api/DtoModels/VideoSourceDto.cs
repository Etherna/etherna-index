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

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class VideoSourceDto
    {
        // Constructors.
        public VideoSourceDto(
            string type,
            string? quality,
            string path,
            long size)
        {
            Type = type;
            Quality = quality;
            Path = path;
            Size = size;
        }

        // Properties.
        public string Type { get; private set; }
        public string? Quality { get; private set; }
        public string Path { get; private set; }
        public long Size { get; private set; }
    }
}
