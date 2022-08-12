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
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Swarm.Models
{
    public class SwarmImageRaw
    {
        // Constructors.
        public SwarmImageRaw(
            float aspectRatio,
            string blurhash,
            IReadOnlyDictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }
        internal SwarmImageRaw(SwarmImageRawSchema1 swarmImageRaw) :
            this(swarmImageRaw.AspectRatio,
                swarmImageRaw.Blurhash,
                swarmImageRaw.Sources)
        { }

        // Properties.
        public float AspectRatio { get; }
        public string Blurhash { get; }
        public IReadOnlyDictionary<string, string> Sources { get; }
    }
}
