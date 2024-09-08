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
using Etherna.EthernaIndex.Domain.Exceptions;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Swarm.DtoModels.ManifestV1;
using Etherna.EthernaIndex.Swarm.DtoModels.ManifestV2;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#if DEBUG_MOCKUP_SWARM
using System.Collections.Generic;
#endif

#if DEBUG_MOCKUP_SWARM
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#endif

namespace Etherna.EthernaIndex.Swarm
{
    public class SwarmService(IBeeClient beeClient) : ISwarmService
    {
        // Fields.
        private readonly JsonSerializerOptions jsonSerializerOptions = new()
        {
            Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
            PropertyNameCaseInsensitive = true,
        };

#if DEBUG_MOCKUP_SWARM
        private readonly Dictionary<SwarmHash, object> SwarmObjectMockups = new(); //hash->object
#endif

        // Methods.
        public async Task<VideoManifestMetadataBase> DeserializeVideoMetadataAsync(
            SwarmHash manifestHash,
            JsonElement jsonElementManifest)
        {
            // Find version.
            var versionStr = jsonElementManifest.TryGetProperty("v", out var jsonVersion) ?
                jsonVersion.GetString()! :
                "1.0"; //first version didn't have an identifier
            var version = new Version(versionStr);

            // Deserialize document.
            return version.Major switch
            {
                1 => DeserializeVideoMetadataV1(jsonElementManifest),
                2 => await DeserializeVideoMetadataV2Async(manifestHash, jsonElementManifest),
                _ => throw new VideoManifestValidationException(new[] { new ValidationError(ValidationErrorType.JsonConvert, "Invalid version") })
            };
        }

        public async Task<VideoManifestMetadataBase> GetVideoMetadataAsync(SwarmHash manifestHash)
        {
#if !DEBUG_MOCKUP_SWARM
            using var manifestStream = (await BeeClient.GetFileAsync(manifestHash)).Stream;
            var jsonElementManifest = await JsonSerializer.DeserializeAsync<JsonElement>(manifestStream);

            return await DeserializeVideoMetadataAsync(manifestHash, jsonElementManifest);
#else
            return (VideoManifestMetadataBase)SwarmObjectMockups[manifestHash];
#endif
        }

#if DEBUG_MOCKUP_SWARM
        public void SetupHashMockup(string hash, object returnedObject) =>
            SwarmObjectMockups[hash] = returnedObject;

        public VideoManifestMetadataBase SetupNewMetadataV2VideoMockup(string manifestHash)
        {
            var manifest = new VideoManifestMetadataV2(
                "Mocked sample video",
                "Test description",
                420,
                [new VideoSourceV2("sources/playlist.m3u8", "720", 100000000, "hls")],
                new ThumbnailV2(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", [new ImageSourceV2(480, "thumbs/myThumb.jpg", "jpeg")]),
                1.77f,
                "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                123456,
                234567,
                $$"""{"test":"sample"}""");

            SetupHashMockup(manifestHash, manifest);

            return manifest;
        }
#endif

        // Helpers.
        private VideoManifestMetadataV1 DeserializeVideoMetadataV1(JsonElement jsonElementManifest)
        {
            var manifestDto = jsonElementManifest.Deserialize<VideoManifestV1Dto>(jsonSerializerOptions)
                ?? throw new VideoManifestValidationException([new ValidationError(ValidationErrorType.JsonConvert, "Empty json")]);

            return new VideoManifestMetadataV1(
                manifestDto.Title,
                manifestDto.Description,
                manifestDto.Duration,
                manifestDto.Sources.Select(s => new VideoSourceV1(s.Bitrate, s.Quality, s.Reference, s.Size)),
                manifestDto.Thumbnail is null ? null :
                    new ThumbnailV1(
                        manifestDto.Thumbnail.AspectRatio,
                        manifestDto.Thumbnail.Blurhash,
                        manifestDto.Thumbnail.Sources.ToDictionary(s => s.Key, s => (SwarmHash)s.Value)),
                manifestDto.BatchId is null ? (PostageBatchId?)null : PostageBatchId.FromString(manifestDto.BatchId),
                manifestDto.CreatedAt,
                manifestDto.UpdatedAt,
                manifestDto.PersonalData);
        }

        private async Task<VideoManifestMetadataV2> DeserializeVideoMetadataV2Async(
            SwarmHash manifestHash,
            JsonElement jsonElementManifest)
        {
            // Get preview dto.
            var manifestPreviewDto = jsonElementManifest.Deserialize<VideoManifestPreviewV2Dto>(jsonSerializerOptions)
                ?? throw new VideoManifestValidationException([new ValidationError(ValidationErrorType.JsonConvert, "Empty json preview")]);

            // Get detail dto.
            using var manifestDetailStream = (await beeClient.GetFileAsync($"{manifestHash}/details")).Stream;
            var manifestDetailDto = await JsonSerializer.DeserializeAsync<VideoManifestDetailV2Dto>(
                manifestDetailStream,
                jsonSerializerOptions) ??
                throw new VideoManifestValidationException([new ValidationError(ValidationErrorType.JsonConvert, "Empty json detail")]);

            return new VideoManifestMetadataV2(
                manifestPreviewDto.Title,
                manifestDetailDto.Description,
                manifestPreviewDto.Duration,
                manifestDetailDto.Sources?.Select(s => new VideoSourceV2(s.Path, s.Quality, s.Size, s.Type)) ?? Array.Empty<VideoSourceV2>(),
                manifestPreviewDto.Thumbnail is null ? null :
                    new ThumbnailV2(
                        manifestPreviewDto.Thumbnail.AspectRatio,
                        manifestPreviewDto.Thumbnail.Blurhash,
                        manifestPreviewDto.Thumbnail.Sources.Select(s => new ImageSourceV2(s.Width, s.Path, s.Type))),
                manifestDetailDto.AspectRatio,
                manifestDetailDto.BatchId,
                manifestPreviewDto.CreatedAt,
                manifestPreviewDto.UpdatedAt,
                manifestDetailDto.PersonalData);
        }
    }
}

#if DEBUG_MOCKUP_SWARM
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
#endif
