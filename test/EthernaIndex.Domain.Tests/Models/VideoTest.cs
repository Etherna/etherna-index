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
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Etherna.EthernaIndex.Domain.Models
{
    public class VideoTest
    {
        // Fields.
        readonly string address = "0x300a31dBAB42863F4b0bEa3E03d0aa89D47DB3f0";
        readonly string encryptKey = "1d111a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f1";
        readonly string manifestHash = "5d942a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        readonly string secondManifestHash = "2b678a1d73fd8f28d71e6b03d2e42f44721db94b734c2edcfe6fcd48b76a74f9";
        readonly User owner;
        readonly Video video;

        // Constructors.
        public VideoTest()
        {
            owner = new User(address);
            video = new Video(encryptKey, EncryptionType.AES256, new VideoManifest(manifestHash), owner);
        }

        // Tests.
        [Fact]
        public void Create_Video()
        {
            //Arrange

            //Act

            //Assert
            Assert.Equal(manifestHash, video.ManifestHash.Hash);
            Assert.Equal(encryptKey, video.EncryptionKey);
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(EncryptionType.AES256, video.EncryptionType);
            Assert.Equal(manifestHash, video.ManifestHash.Hash);
            Assert.NotNull(video.Owner);
            Assert.Equal(address, video.Owner.Address);
            var manifest = video.GetManifest();
            Assert.NotNull(manifest);
            Assert.Equal(manifestHash, manifest.ManifestHash);
            Assert.Null(manifest.IsValid);
            Assert.Null(manifest.ValidationTime);
        }

        [Fact]
        public void UpdateVideo_AddOnlyManifestToValidate()
        {
            //Arrange

            //Act
            video.UpdateManifest(new VideoManifest(secondManifestHash));

            //Assert
            Assert.Equal(manifestHash, video.ManifestHash.Hash);
            Assert.Equal(encryptKey, video.EncryptionKey);
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(0, video.TotDownvotes);
            Assert.Equal(EncryptionType.AES256, video.EncryptionType);
            Assert.Equal(manifestHash, video.ManifestHash.Hash);
            Assert.NotNull(video.Owner);
            Assert.Equal(address, video.Owner.Address);
            Assert.Equal(2, video.VideoManifest.Count());
            Assert.Contains(video.VideoManifest,
                i => i.ManifestHash == manifestHash);
            Assert.Contains(video.VideoManifest,
                i => i.ManifestHash == secondManifestHash);
            var manifest = video.GetManifest();
            Assert.NotNull(manifest);
            Assert.Equal(manifestHash, manifest.ManifestHash);
            Assert.Null(manifest.IsValid);
            Assert.Null(manifest.ValidationTime);
        }


    }
}
