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
using Etherna.EthernaIndex.Domain.Models.ValidationResults;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class ValidationResult : EntityModelBase<string>
    {
        // Fields.
        private List<ErrorValidationResult> _errorValidationResults = new();

        // Constructors.
        protected ValidationResult(
            string manifestHash,
            User owner)
        {
            if (manifestHash is null)
            {
                throw new ArgumentNullException(nameof(manifestHash));
            }
            if (owner is null)
            {
                throw new ArgumentNullException(nameof(owner));
            }

            Checked = false;
            IsInizialized = false;
            ManifestHash = new SwarmContentHash(manifestHash);
            Owner = owner;
        }
        protected ValidationResult() { }

        // Properties.
        public bool Checked { get; private set; }
        public virtual IEnumerable<ErrorValidationResult> ErrorValidationResults
        {
            get => _errorValidationResults;
            protected set => _errorValidationResults = new List<ErrorValidationResult>(value ?? Array.Empty<ErrorValidationResult>());
        }
        public bool IsInizialized { get; protected set; }
        public bool? IsValid { get; private set; } = default!;
        public DateTime? LastCheck { get; private set; }
        public SwarmContentHash ManifestHash { get; init; } = default!;
        public User Owner { get; init; } = default!;

        // Methods.
        [PropertyAlterer(nameof(Checked))]
        [PropertyAlterer(nameof(ErrorValidationResults))]
        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(LastCheck))]
        public virtual void SetResult(Dictionary<ValidationError, string> validationErrors)
        {
            if (!IsInizialized &&
                (validationErrors is null ||
                !validationErrors.ContainsKey(ValidationError.JsonConvert))//in case of JsonConvert error isn't possible call InizializeManifest()
                ) 
            {
                throw new InvalidOperationException("InizializeManifest() before SetResult()");
            }

            Checked = true;
            LastCheck = DateTime.Now;
            if (validationErrors is null ||
                !validationErrors.Any())
            {
                IsValid = true;
                _errorValidationResults.Clear();
                return;
            }

            IsValid = false;
            foreach (var item in validationErrors)
            {
                _errorValidationResults.Add(new ErrorValidationResult(item.Key, item.Value, LastCheck.Value));
            }
        }
    }
}
