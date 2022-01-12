using Etherna.EthernaIndex.Domain.Models.ValidationResults;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class ValidationResult : EntityModelBase<string>
    {
        // Fields.
        private List<ErrorValidationResult> _errorValidationResults = new();

        // Constructors.
        public ValidationResult(
            string manifestHash)
        {
            if (manifestHash is null)
            {
                throw new ArgumentNullException(nameof(manifestHash));
            }

            ManifestHash = manifestHash;
        }
        protected ValidationResult() { }



        // Properties.
        public string ManifestHash { get; init; } = default!;
        public DateTime? LastCheck { get; private set; }
        public bool Checked { get; private set; } = default!;
        public bool IsValid { get; private set; } = default!;
        public virtual IEnumerable<ErrorValidationResult> ErrorValidationResults
        {
            get => _errorValidationResults;
            protected set => _errorValidationResults = new List<ErrorValidationResult>(value ?? Array.Empty<ErrorValidationResult>());
        }

        // Methods.
        [PropertyAlterer(nameof(Checked))]
        [PropertyAlterer(nameof(ErrorValidationResults))]
        [PropertyAlterer(nameof(IsValid))]
        [PropertyAlterer(nameof(LastCheck))]
        public virtual void SetResult(Dictionary<ValidationError, string> validationErrors)
        {
            Checked = true;
            LastCheck = DateTime.Now;
            if (validationErrors == null ||
                validationErrors.Any())
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
