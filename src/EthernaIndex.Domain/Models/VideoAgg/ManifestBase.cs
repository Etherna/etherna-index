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

using Etherna.EthernaIndex.Domain.Models.Swarm;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public abstract class ManifestBase : EntityModelBase<string>
    {
        // Fields.
        private List<ErrorDetail> _validationErrors = new();

        // Constructors.
        protected ManifestBase(string manifestHash)
        {
            Manifest = new SwarmBzz(manifestHash);
        }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ManifestBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual bool? IsValid { get; private set; }
        public virtual SwarmBzz Manifest { get; protected set; }
        public virtual IEnumerable<ErrorDetail> ValidationErrors
        {
            get => _validationErrors;
            protected set => _validationErrors = new List<ErrorDetail>(value ?? Array.Empty<ErrorDetail>());
        }
        public virtual DateTime? ValidationTime { get; private set; }

        // Methods.
        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(ValidationErrors))]
        [PropertyAlterer(nameof(ValidationTime))]
        internal virtual void FailedValidation(IEnumerable<ErrorDetail> errorDetails)
        {
            IsValid = false;
            ValidationTime = DateTime.UtcNow;
            _validationErrors.AddRange(errorDetails);
        }

        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(ValidationErrors))]
        [PropertyAlterer(nameof(ValidationTime))]
        internal virtual void SucceededValidation()
        {
            IsValid = true;
            ValidationTime = DateTime.UtcNow;
            _validationErrors.Clear();
        }
    }
}
