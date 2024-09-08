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

namespace Etherna.EthernaIndex.Domain.Models
{
    public enum VoteValue { Up, Down, Neutral }

    public class VideoVote : EntityModelBase<string>
    {
        // Contructors.
        public VideoVote(
            User owner,
            Video video,
            VoteValue value)
        {
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Value = value;
            Video = video ?? throw new ArgumentNullException(nameof(video));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected VideoVote() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual User Owner { get; protected set; }
        public virtual VoteValue Value { get; protected set; }
        public virtual Video Video { get; protected set; }
    }
}
