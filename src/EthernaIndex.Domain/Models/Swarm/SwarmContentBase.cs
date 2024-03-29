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

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Etherna.EthernaIndex.Domain.Models.Swarm
{
    public abstract partial class SwarmContentBase : ModelBase
    {
        // Consts.
        [GeneratedRegex("^[A-Fa-f0-9]{64}$")]
        private static partial Regex SwarmHashRegex();

        // Constructors.
        protected SwarmContentBase(
            string hash)
        {
            if (hash is null)
                throw new ArgumentNullException(nameof(hash));
            if (!SwarmHashRegex().IsMatch(hash))
                throw new ArgumentException($"{hash} is not a valid swarm hash", nameof(hash));

            Hash = hash;
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected SwarmContentBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual string Hash { get; protected set; }

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            if (EqualityComparer<string>.Default.Equals(Hash, default) ||
                obj is not SwarmContentBase ||
                EqualityComparer<string>.Default.Equals((obj as SwarmContentBase)!.Hash, default)) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<string>.Default.Equals(Hash, (obj as SwarmContentBase)!.Hash);
        }

        public override int GetHashCode()
        {
            if (EqualityComparer<string>.Default.Equals(Hash, default))
                return -1;
            return Hash.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }
    }
}
