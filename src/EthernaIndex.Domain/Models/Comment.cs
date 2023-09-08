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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Comment : EntityModelBase<string>
    {
        // Fields.
        private Dictionary<DateTime, string> _textHistory = new();

        // Consts.
        public const int MaxEditHistory = 10;
        public static readonly TimeSpan MaxEditTimeSpan = TimeSpan.FromHours(24);
        public const int MaxLength = 2000;
        public const string RemovedByAuthorReplaceText = "(removed by author)";
        public const string RemovedByModeratorReplaceText = "(removed by moderator)";

        // Constructors.
        public Comment(
            User author,
            string text,
            Video video)
        {
            Author = author ?? throw new ArgumentNullException(nameof(author));
            _textHistory.Add(DateTime.UtcNow, text ?? throw new ArgumentNullException(nameof(text)));
            Video = video ?? throw new ArgumentNullException(nameof(video));

            if (text.Length > MaxLength)
                throw new ArgumentOutOfRangeException(nameof(text));
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected Comment() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual User Author { get; protected set; }
        public virtual bool IsEditable =>
            !IsFrozen && 
            DateTime.UtcNow <= CreationDateTime + MaxEditTimeSpan &&
            _textHistory.Count < MaxEditHistory;
        public virtual bool IsFrozen { get; protected set; }
        public virtual string LastText => TextHistory.MaxBy(pair => pair.Key).Value;
        public virtual DateTime LastUpdateDateTime => TextHistory.Keys.Max();
        public virtual IReadOnlyDictionary<DateTime, string> TextHistory
        {
            get => _textHistory;
            protected set => _textHistory = new Dictionary<DateTime, string>(value ?? new Dictionary<DateTime, string>());
        }
        public virtual Video Video { get; protected set; }

        // Methods.
        [PropertyAlterer(nameof(LastText))]
        [PropertyAlterer(nameof(LastUpdateDateTime))]
        [PropertyAlterer(nameof(TextHistory))]
        public virtual void EditByAuthor(string text)
        {
            if (!IsEditable)
                throw new InvalidOperationException();

            _textHistory.Add(DateTime.UtcNow, text);
        }
        
        [PropertyAlterer(nameof(IsFrozen))]
        [PropertyAlterer(nameof(LastText))]
        [PropertyAlterer(nameof(LastUpdateDateTime))]
        [PropertyAlterer(nameof(TextHistory))]
        public virtual void SetAsDeletedByAuthor()
        {
            if (IsFrozen)
                throw new InvalidOperationException();
            
            _textHistory.Clear();
            _textHistory.Add(DateTime.UtcNow, RemovedByAuthorReplaceText);
            IsFrozen = true;
        }

        [PropertyAlterer(nameof(IsFrozen))]
        [PropertyAlterer(nameof(LastText))]
        [PropertyAlterer(nameof(LastUpdateDateTime))]
        [PropertyAlterer(nameof(TextHistory))]
        public virtual void SetAsDeletedByModerator()
        {
            if (IsFrozen)
                throw new InvalidOperationException();

            _textHistory.Clear();
            _textHistory.Add(DateTime.UtcNow, RemovedByModeratorReplaceText);
            IsFrozen = true;
        }
    }
}
