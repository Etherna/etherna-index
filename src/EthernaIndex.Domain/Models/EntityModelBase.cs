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

using Etherna.DomainEvents;
using Etherna.MongODM.Core.Domain.Models;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class EntityModelBase : ModelBase, IEntityModel
    {
        private DateTime _creationDateTime;
        private readonly HashSet<IDomainEvent> _events = new();

        // Constructors and dispose.
        protected EntityModelBase()
        {
            _creationDateTime = DateTime.UtcNow;
        }

        public virtual void DisposeForDelete() { }

        // Properties.
        public virtual DateTime CreationDateTime { get => _creationDateTime; protected set => _creationDateTime = value; }
        public virtual IReadOnlyCollection<IDomainEvent> Events => _events;

        // Methods.
        public void AddEvent(IDomainEvent e) => _events.Add(e);

        public void ClearEvents() => _events.Clear();
    }

    public abstract class EntityModelBase<TKey> : EntityModelBase, IEntityModel<TKey>
    {
        // Properties.
        public virtual TKey Id { get; protected set; } = default!;

        // Methods.
        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj is null) return false;
            if (EqualityComparer<TKey>.Default.Equals(Id, default) ||
                obj is not IEntityModel<TKey> ||
                EqualityComparer<TKey>.Default.Equals((obj as IEntityModel<TKey>)!.Id, default)) return false;
            return GetType() == obj.GetType() &&
                EqualityComparer<TKey>.Default.Equals(Id, (obj as IEntityModel<TKey>)!.Id);
        }

        public override int GetHashCode()
        {
            if (EqualityComparer<TKey>.Default.Equals(Id, default))
                return -1;
            return Id!.GetHashCode();
        }
    }
}
