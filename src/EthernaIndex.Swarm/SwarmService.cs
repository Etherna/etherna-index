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

using Etherna.BeeNet;
using Etherna.BeeNet.Clients.GatewayApi;
using Etherna.EthernaIndex.Domain.Exceptions;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV1;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Swarm.DtoModels.ManifestV1;
using Etherna.EthernaIndex.Swarm.DtoModels.ManifestV2;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

#if DEBUG_MOCKUP_SWARM
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
#endif

#if DEBUG_MOCKUP_SWARM
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
#endif

namespace Etherna.EthernaIndex.Swarm
{
    public class SwarmService : ISwarmService
    {
        // Fields.
        private readonly IBeeNodeClient BeeNodeClient;
        private readonly SwarmSettings SwarmSettings;

#if DEBUG_MOCKUP_SWARM
        private readonly Dictionary<string, object> SwarmObjectMockups = new(); //hash->object
        private readonly Random random = new();
#endif

        // Constructors.
        public SwarmService(IOptions<SwarmSettings> swarmSettings)
        {
            if (swarmSettings?.Value is null)
                throw new ArgumentNullException(nameof(swarmSettings));

            SwarmSettings = swarmSettings.Value;
            BeeNodeClient = new BeeNodeClient(
                SwarmSettings.GatewayUrl,
                gatewayApiVersion: GatewayApiVersion.v4_0_0);
        }

        // Methods.
        public async Task<VideoManifestMetadataBase> DeserializeVideoMetadataAsync(
            string manifestHash,
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

        public async Task<VideoManifestMetadataBase> GetVideoMetadataAsync(string manifestHash)
        {
            if (BeeNodeClient.GatewayClient is null)
                throw new InvalidOperationException(nameof(BeeNodeClient.GatewayClient));

#if !DEBUG_MOCKUP_SWARM
            using var manifestStream = await BeeNodeClient.GatewayClient.GetFileAsync(manifestHash);
            var jsonElementManifest = await JsonSerializer.DeserializeAsync<JsonElement>(manifestStream);

            return await DeserializeVideoMetadataAsync(manifestHash, jsonElementManifest);
#else
            return (VideoManifestMetadataBase)SwarmObjectMockups[manifestHash];
#endif
        }

#if DEBUG_MOCKUP_SWARM
        [SuppressMessage("Security", "CA5394:Do not use insecure randomness", Justification = "Not critical")]
        public string GenerateNewHash()
        {
            var digits = 64;

            byte[] buffer = new byte[digits / 2];
            random.NextBytes(buffer);
            string result = string.Concat(buffer.Select(x => x.ToString("X2", CultureInfo.InvariantCulture)).ToArray());
            if (digits % 2 == 0)
                return result;
            return result + random.Next(16).ToString("X", CultureInfo.InvariantCulture);
        }

        public void SetupHashMockup(string hash, object returnedObject) =>
            SwarmObjectMockups[hash] = returnedObject;

        public VideoManifestMetadataBase SetupNewMetadataV1VideoMockup(string manifestHash)
        {
            var manifest = new VideoManifestMetadataV1(
                "Mocked sample video",
                "Test description",
                420,
                new[] { new VideoSourceV1(42, "720", GenerateNewHash(), 100000000) },
                new ThumbnailV1(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new Dictionary<string, string>() { { "480", GenerateNewHash() } }),
                "36b7efd913ca4cf880b8eeac5093fa27b0825906c600685b6abdd6566e6cfe8f",
                123456,
                234567,
                $$"""{"test":"sample"}""");

            SetupHashMockup(manifestHash, manifest);

            return manifest;
        }

        public VideoManifestMetadataBase SetupNewMetadataV2VideoMockup(string manifestHash)
        {
            var manifest = new VideoManifestMetadataV2(
                "Mocked sample video",
                "Test description",
                420,
                new[] { new VideoSourceV2(GenerateNewHash(), "720", 100000000, "mp4") },
                new ThumbnailV2(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj", new[] { new ImageSourceV2(480, GenerateNewHash(), "jpeg") }),
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
            var manifestDto = jsonElementManifest.Deserialize<VideoManifestV1Dto>(
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                    PropertyNameCaseInsensitive = true,
                }) ?? throw new VideoManifestValidationException(new[] { new ValidationError(ValidationErrorType.JsonConvert, "Empty json") });

            return new VideoManifestMetadataV1(
                manifestDto.Title,
                manifestDto.Description,
                manifestDto.Duration,
                manifestDto.Sources.Select(s => new VideoSourceV1(s.Bitrate, s.Quality, s.Reference, s.Size)),
                manifestDto.Thumbnail is null ? null :
                    new ThumbnailV1(
                        manifestDto.Thumbnail.AspectRatio,
                        manifestDto.Thumbnail.Blurhash,
                        manifestDto.Thumbnail.Sources),
                manifestDto.BatchId,
                manifestDto.CreatedAt,
                manifestDto.UpdatedAt,
                manifestDto.PersonalData);
        }

        private async Task<VideoManifestMetadataV2> DeserializeVideoMetadataV2Async(
            string manifestHash,
            JsonElement jsonElementManifest)
        {
            if (BeeNodeClient.GatewayClient is null)
                throw new InvalidOperationException(nameof(BeeNodeClient.GatewayClient));

            // Get preview dto.
            var manifestPreviewDto = jsonElementManifest.Deserialize<VideoManifestPreviewV2Dto>(
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                    PropertyNameCaseInsensitive = true,
                }) ?? throw new VideoManifestValidationException(new[] { new ValidationError(ValidationErrorType.JsonConvert, "Empty json preview") });

            // Get detail dto.
            using var manifestDetailStream = await BeeNodeClient.GatewayClient.GetFileAsync($"{manifestHash}/details");
            var manifestDetailDto = await JsonSerializer.DeserializeAsync<VideoManifestDetailV2Dto>(
                manifestDetailStream,
                new JsonSerializerOptions
                {
                    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
                    PropertyNameCaseInsensitive = true,
                }) ??
                throw new VideoManifestValidationException(new[] { new ValidationError(ValidationErrorType.JsonConvert, "Empty json detail") });

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
