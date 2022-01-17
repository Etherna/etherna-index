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

using Etherna.MongODM.Core.Attributes;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.ValidationResults
{
    public class SwarmImageRaw
    {
        // Fields.
        private Dictionary<string, string> _sources = new();

        // Constructors.
        public SwarmImageRaw(
            int aspectRatio,
            string blurhash,
            Dictionary<string, string> sources)
        {
            AspectRatio = aspectRatio;
            Blurhash = blurhash;
            Sources = sources;
        }
        protected SwarmImageRaw() { }

        public int AspectRatio { get; set; }
        public string Blurhash { get; set; } = default!;
#pragma warning disable CA2227 // Collection properties should be read only
        public virtual IDictionary<string, string> Sources
#pragma warning restore CA2227 // Collection properties should be read only
        {
            get => _sources;
            protected set => _sources = new Dictionary<string, string>(value ?? new Dictionary<string, string>());
        }

        // Methods.
        [PropertyAlterer(nameof(Sources))]
        public virtual void AddSource(string sourceType, string reference)
        {
            if (!_sources.TryAdd(reference, reference))
            {
                _sources[sourceType] = reference;
            }
        }

        [PropertyAlterer(nameof(Sources))]
        public virtual void DeleteSource(string sourceType, string reference)
        {
            _sources.Remove(sourceType);
        }
    }
}
