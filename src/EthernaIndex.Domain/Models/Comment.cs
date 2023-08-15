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

using Etherna.EthernaIndex.Domain.Models.CommentAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Comment : EntityModelBase<string>
    {
        // Fields.
        private List<HistoryComment> _histories = new();

        // Consts.
        public const int MaxLength = 2000;

        // Constructors.
        public Comment(
            User author,
            string text,
            Video video)
        {
            Author = author ?? throw new ArgumentNullException(nameof(author));
            LastUpdateDateTime = DateTime.UtcNow;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Video = video ?? throw new ArgumentNullException(nameof(video));

            if (text.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(text));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected Comment() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual User Author { get; protected set; }
        public virtual IEnumerable<HistoryComment> Histories
        {
            get => _histories;
            protected set => _histories = new List<HistoryComment>(value ?? new List<HistoryComment>());
        }
        public virtual bool IsFrozen { get; set; }
        public virtual DateTime LastUpdateDateTime { get; protected set; }
        public virtual string Text { get; protected set; }
        public virtual Video Video { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(Histories))]
        [PropertyAlterer(nameof(IsFrozen))]
        [PropertyAlterer(nameof(LastUpdateDateTime))]
        [PropertyAlterer(nameof(Text))]
        public virtual void SetAsDeletedByAuthor()
        {
            if (IsFrozen)
                throw new InvalidOperationException();

            _histories.Add(new HistoryComment(null, Text));
            IsFrozen = true;
            LastUpdateDateTime = DateTime.UtcNow;
            Text = "(removed by author)";
        }

        [PropertyAlterer(nameof(Histories))]
        [PropertyAlterer(nameof(IsFrozen))]
        [PropertyAlterer(nameof(LastUpdateDateTime))]
        [PropertyAlterer(nameof(Text))]
        public virtual void SetAsDeletedByModerator(User userModerator)
        {
            if (IsFrozen)
                throw new InvalidOperationException();

            _histories.Add(new HistoryComment(userModerator, Text));
            IsFrozen = true;
            LastUpdateDateTime = DateTime.UtcNow;
            Text = "(removed by moderator)";
        }

        [PropertyAlterer(nameof(Histories))]
        [PropertyAlterer(nameof(LastUpdateDateTime))]
        [PropertyAlterer(nameof(Text))]
        public virtual void UpdateComment(string text, HistoryComment historyComment)
        {
            if (IsFrozen)
                throw new InvalidOperationException();

            _histories.Add(historyComment);
            LastUpdateDateTime = DateTime.UtcNow;
            Text = text;
        }
    }
}
