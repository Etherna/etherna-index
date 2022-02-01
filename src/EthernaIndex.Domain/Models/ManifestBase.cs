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

using Etherna.EthernaIndex.Domain.Models.Manifest;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class ManifestBase : EntityModelBase<string>
    {
        // Fields.
        private List<ErrorDetail> _errorValidationResults = new();

        // Constructors.
        protected ManifestBase(string manifestHash)
        {
            if (manifestHash is null)
                throw new ArgumentNullException(nameof(manifestHash));

            ManifestHash = manifestHash;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        protected ManifestBase() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Properties.
        public virtual IEnumerable<ErrorDetail> ErrorValidationResults
        {
            get => _errorValidationResults;
            protected set => _errorValidationResults = new List<ErrorDetail>(value ?? Array.Empty<ErrorDetail>());
        }
        public virtual bool? IsValid { get; private set; }
        public virtual string ManifestHash { get; private set; }
        public virtual DateTime? ValidationTime { get; private set; }

        // Methods.
        [PropertyAlterer(nameof(ErrorValidationResults))]
        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(ValidationTime))]
        public virtual void FailedValidation(IEnumerable<ErrorDetail> errorDetails)
        {
            ValidationTime = DateTime.UtcNow;
            IsValid = false;
            _errorValidationResults.AddRange(errorDetails);
        }

        [PropertyAlterer(nameof(ErrorValidationResults))]
        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(ValidationTime))]
        protected virtual void SuccessfulValidation()
        {
            ValidationTime = DateTime.UtcNow;
            IsValid = true;
            _errorValidationResults.Clear();
        }

    }
}
