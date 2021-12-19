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
        protected VideoVote() { }

        // Properties.
        public virtual User Owner { get; protected set; } = default!;
        public virtual VoteValue Value { get; protected set; }
        public virtual Video Video { get; protected set; } = default!;
    }
}