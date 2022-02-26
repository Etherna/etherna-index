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

using Etherna.MongODM.Core.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class Video : EntityModelBase<string>
    {
        // Fields.
        private List<VideoManifest> _videoManifests = new();

        // Constructors and dispose.
        public Video(
            string? encryptionKey,
            EncryptionType encryptionType,
            User owner)
        {
            SetEncryptionKey(encryptionKey, encryptionType);
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
        public virtual VideoManifest? LastValidManifest
        {
            get
            {
                return _videoManifests
                    .Where(i => i.IsValid == true)
                    .OrderByDescending(i => i.CreationDateTime)
                    .FirstOrDefault();
            }
        }
        public virtual User Owner { get; protected set; } = default!;
        public virtual string? Title
        {
            get
            {
                return LastValidManifest?.Title;
            }
        }
        public virtual long TotDownvotes { get; set; }
        public virtual long TotUpvotes { get; set; }
        public virtual VideoValidationStatus ValidationStatus { get; set; }
        public virtual IEnumerable<VideoManifest> VideoManifests
        {
            get => _videoManifests;
            protected set => _videoManifests = new List<VideoManifest>(value ?? new List<VideoManifest>());
        }

        // Methods.

        [PropertyAlterer(nameof(VideoManifests))]
        public virtual void AddManifest(VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            if (videoManifest.ValidationTime is null)
            {
                var ex = new InvalidOperationException("Manifest not validated");
                ex.Data.Add("ManifestHash", videoManifest.Manifest.Hash);
                throw ex;
            }

            if (_videoManifests.Any(i => i.Manifest.Hash == videoManifest.Manifest.Hash))
            {
                var ex = new InvalidOperationException("AddManifest duplicate");
                ex.Data.Add("ManifestHash", videoManifest.Manifest.Hash);
                throw ex;
            }

            if (ValidationStatus == VideoValidationStatus.WaitingValidManifest &&
                videoManifest.IsValid == true)
                ValidationStatus = VideoValidationStatus.Valid;

            _videoManifests.Add(videoManifest);
        }

        [PropertyAlterer(nameof(VideoManifests))]
        public virtual bool RemoveManifest(VideoManifest videoManifest)
        {
            if (videoManifest is null)
                throw new ArgumentNullException(nameof(videoManifest));

            if (ValidationStatus == VideoValidationStatus.Valid &&
                videoManifest.IsValid == true)
                ValidationStatus = VideoValidationStatus.WaitingValidManifest;

            return _videoManifests.Remove(videoManifest);
        }

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

    }
}
