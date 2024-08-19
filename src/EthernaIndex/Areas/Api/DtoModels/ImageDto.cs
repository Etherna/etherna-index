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

using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    [Obsolete("Used only for API backwards compatibility")]
    public class ImageDto
    {
        // Constructors.
        public ImageDto(
            float aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }

        // Properties.
        public float AspectRatio { get; private set; }
        public string Blurhash { get; private set; }
        public IReadOnlyDictionary<string, string> Sources { get; private set; }
    }
}
