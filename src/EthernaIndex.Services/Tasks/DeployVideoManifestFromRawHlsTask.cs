// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Etherna.BeeNet;
using Etherna.BeeNet.Models;
using Etherna.Sdk.Tools.Video.Models;
using Etherna.Sdk.Tools.Video.Services;
using Etherna.UniversalFiles;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    public class DeployVideoManifestFromRawHlsTask(
        IBeeClient beeClient,
        IHlsService hlsService,
        IUFileProvider uFileProvider) :
        IDeployVideoManifestFromRawHlsTask
    {
        public Task RunAsync(
            string videoId,
            string hlsRawPlaylistAddress,
            string? thumbnailRawAddress,
            string title,
            string description,
            int durationSeconds) =>
            RunHelperAsync(
                videoId,
                SwarmAddress.FromString(hlsRawPlaylistAddress),
                thumbnailRawAddress is null ? (SwarmAddress?)null : SwarmAddress.FromString(thumbnailRawAddress),
                title,
                description,
                TimeSpan.FromSeconds(durationSeconds));
        
        // Helpers.
        public async Task<VideoEncodingBase> DecodeVideoEncodingFromSwarmAddressAsync(
            TimeSpan duration,
            SwarmAddress swarmAddress)
        {
            var mainFileUri = new SwarmUUri(swarmAddress, UUriKind.Absolute);
            var mainFile = await FileBase.BuildFromUFileAsync(
                uFileProvider.BuildNewUFile(mainFileUri));
            
            // Get main file directory.
            var masterFileDirectory = mainFileUri.TryGetParentDirectoryAsAbsoluteUri();
            if (masterFileDirectory is null)
                throw new InvalidOperationException($"Can't get parent directory of {mainFileUri.OriginalUri}");

            var chunkRef = await beeClient.ResolveAddressToChunkReferenceAsync(swarmAddress);
            mainFile.SwarmHash = chunkRef.Hash;
            
            //if is a master playlist
            var masterPlaylist = await hlsService.TryParseHlsMasterPlaylistFromLocalFileAsync(mainFile);
            if (masterPlaylist is null)
                throw new InvalidOperationException("Only master file is supported");
                
            return await hlsService.ParseVideoEncodingFromHlsMasterPlaylistLocalFileAsync(
                duration,
                mainFile,
                swarmAddress,
                masterPlaylist);
        }
        
        private async Task RunHelperAsync(
            string videoId,
            SwarmAddress hlsRawPlaylistAddress,
            SwarmAddress? thumbnailRawAddress,
            string title,
            string description,
            TimeSpan duration)
        {
            // Decode video encoding.
            var videoEncoding = await DecodeVideoEncodingFromSwarmAddressAsync(duration, hlsRawPlaylistAddress);
            
            // Create manifest.
            
            // Deploy manifest.
            
            // Update video.
            
        }
    }
}