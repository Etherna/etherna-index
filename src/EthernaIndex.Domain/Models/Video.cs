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
using Etherna.MongODM.Attributes;
using System;
using System.Text.RegularExpressions;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Constructors and dispose.
        public Video(
            string? encryptionKey,
            EncryptionType encryptionType,
            SwarmContentHash manifestHash,
            User owner)
        {
            SetEncryptionKey(encryptionKey, encryptionType);
            SetManifestHash(manifestHash);
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));
            Owner.AddVideo(this);
        }
        protected Video() { }

        public override void DisposeForDelete()
        {
            //owner channel
            Owner.RemoveVideo(this);

            base.DisposeForDelete();
        }

        // Properties.
        public virtual string? EncryptionKey { get; protected set; }
        public virtual EncryptionType EncryptionType { get; protected set; }
        public virtual SwarmContentHash ManifestHash { get; protected set; } = default!;
        public virtual User Owner { get; protected set; } = default!;
        public virtual long TotDownvotes { get; set; }
        public virtual long TotUpvotes { get; set; }

        // Methods.
        [PropertyAlterer(nameof(EncryptionKey))]
        [PropertyAlterer(nameof(EncryptionType))]
        public virtual void SetEncryptionKey(string? encryptionKey, EncryptionType encryptionType)
        {
            switch (encryptionType)
            {
                case EncryptionType.AES256:
                    if (string.IsNullOrEmpty(encryptionKey))
                        throw new ArgumentException($"Encryption key can't be empty with encrypted content");
                    if (!Regex.IsMatch(encryptionKey, "^[A-Fa-f0-9]{64}$"))
                        throw new ArgumentException($"Encryption key is not a valid {encryptionType} key");
                    break;
                case EncryptionType.Plain:
                    if (!string.IsNullOrEmpty(encryptionKey))
                        throw new ArgumentException($"Encryption key must be empty with unencrypted content");
                    break;
                default:
                    throw new InvalidOperationException();
            }

            EncryptionKey = encryptionKey;
            EncryptionType = encryptionType;
        }

        [PropertyAlterer(nameof(ManifestHash))]
        public void SetManifestHash(SwarmContentHash manifestHash)
        {
            ManifestHash = manifestHash ?? throw new ArgumentNullException(nameof(manifestHash));
        }
    }
}
