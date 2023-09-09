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

using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.EthernaIndex.Swarm
{
    public class SwarmServiceTest
    {
        // Internal classes.
        public class ParseManifestTestElement
        {
            public ParseManifestTestElement(
                string rootManifestReference,
                IDictionary<string, string> manifests)
            {
                RootManifestReference = rootManifestReference;
                Manifests = manifests;
            }

            public string RootManifestReference { get; }
            public IDictionary<string, string> Manifests { get; }
        }

        // Fields.
        private readonly ISwarmService swarmService;

        // Constructor.
        public SwarmServiceTest()
        {
            var swarmSettings = new SwarmSettings()
            {
                GatewayUrl = "http://localhost/"
            };

            var swarmServiceOptionsMock = new Mock<IOptions<SwarmSettings>>();
            swarmServiceOptionsMock.Setup(o => o.Value).Returns(swarmSettings);

            swarmService = new SwarmService(swarmServiceOptionsMock.Object);
        }

        // Data.
        public static IEnumerable<object[]> ParseManifestTests
        {
            get
            {
                var tests = new List<ParseManifestTestElement>
                {
                    //v1.1
                    new ParseManifestTestElement(
                        "0000000000000000000000000000000000000000000000000000000000000000",
                        new Dictionary<string, string>()
                        {
                            ["0000000000000000000000000000000000000000000000000000000000000000"] =
                             @"{
                               ""v"": ""1.1"",
                               ""title"": ""Titolo 3"",
                               ""description"": ""test!!!"",
                               ""duration"": 19,
                               ""originalQuality"": ""1954p"",
                               ""ownerAddress"": ""0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246"",
                               ""createdAt"": 1660397733617,
                               ""updatedAt"": 1660397733617,
                               ""thumbnail"": {
                                 ""blurhash"": ""UTHoa;-VEVO=??v]SlOu2ep0slR:kisia*bJ"",
                                 ""aspectRatio"": 1.7777777777777777,
                                 ""sources"": {
                                   ""720w"": ""5d69d94f1ffa17560a88abc4a99aa40b0cabe6012766f51e5c19193887adacb1"",
                                   ""480w"": ""0b7425036143ed65932ac64cd6c4ddb4f2fd3e9bd51ed0f13bd406926c45c325""
                                 }
                               },
                               ""sources"": [
                                 {
                                   ""reference"": ""e44671417466df08d3b67d74a081021ab2bba70224fc0d6e4d00c35d80328c6c"",
                                   ""quality"": ""1954p"",
                                   ""size"": 3739997,
                                   ""bitrate"": 1574736
                                 }
                               ],
                               ""batchId"": ""5d35cbf4cea6349c1f74340ce9f0befd7a60a17426508da7b205871d683a3a23""
                             }"
                        }),

                    //v1.0
                    new ParseManifestTestElement(
                        "0000000000000000000000000000000000000000000000000000000000000000",
                        new Dictionary<string, string>()
                        {
                            ["0000000000000000000000000000000000000000000000000000000000000000"] =
                             @"{
                               ""title"": ""Test 1"",
                               ""description"": ""desc"",
                               ""createdAt"": 1645091199100,
                               ""duration"": 18,
                               ""originalQuality"": ""720p"",
                               ""ownerAddress"": ""0x6163C4b8264a03CCAc412B83cbD1B551B6c6C246"",
                               ""thumbnail"": {
                                 ""blurhash"": ""UTHoa;-VEVO=??v]SlOu2ep0slR:kisia*bJ"",
                                 ""aspectRatio"": 1.7777777777777777,
                                 ""sources"": {
                                   ""720w"": ""5d69d94f1ffa17560a88abc4a99aa40b0cabe6012766f51e5c19193887adacb1"",
                                   ""480w"": ""0b7425036143ed65932ac64cd6c4ddb4f2fd3e9bd51ed0f13bd406926c45c325""
                                 }
                               },
                               ""sources"": [
                                 {
                                   ""quality"": ""720p"",
                                   ""reference"": ""94f4fcb1a902597c2bc53c5b48637af952a99328ec299f33e129740818a9e302"",
                                   ""size"": 448350,
                                   ""bitrate"": 216398
                                 }
                               ],
                               ""v"": ""1.0""
                             }"
                        })
                };

                return tests.Select(t => new object[] { t });
            }
        }

        // Tests.
        [Theory, MemberData(nameof(ParseManifestTests))]
        public async Task ParseManifestAsync(ParseManifestTestElement test)
        {
            if (test is null)
                throw new ArgumentNullException(nameof(test));

            // Action.
            var metadata = await swarmService.DeserializeVideoMetadataAsync(
                test.RootManifestReference,
                JsonSerializer.Deserialize<JsonElement>(test.Manifests[test.RootManifestReference]));

            // Assert.
            Assert.NotNull(metadata);
        }
    }
}
