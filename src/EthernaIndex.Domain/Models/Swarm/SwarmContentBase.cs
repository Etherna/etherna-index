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
            ArgumentNullException.ThrowIfNull(hash, nameof(hash));
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
