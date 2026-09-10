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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg.ManifestV2;
using Etherna.EthernaIndex.Services.Tasks;
using Etherna.SwarmSdk.Models;
using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Moq;
using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.EthernaIndex.Areas.Admin.Pages.VideoManifests
{
    public class ManifestModelTest
    {
        // Fields.
        private readonly Mock<IBackgroundJobClient> backgroundJobClientMock = new();
        private readonly Mock<IIndexDbContext> indexDbContextMock = new();
        private readonly ManifestModel manifestModel;
        private readonly SwarmReference manifestReference = "af4b8b977a36938bfbc62ed2f9014883e3ae651e3698b16366bd29a5ad228863";
        private readonly Mock<VideoManifest> videoManifestMock;
        private readonly Mock<Video> videoMock = new();

        // Constructor.
        public ManifestModelTest()
        {
            videoManifestMock = new Mock<VideoManifest>(manifestReference) { CallBase = true };
            videoMock.Setup(v => v.Id).Returns("videoId");

            manifestModel = new ManifestModel(backgroundJobClientMock.Object, indexDbContextMock.Object);
        }

        // Tests.
        [Fact]
        public async Task OnGetAsync_WhenManifestIsFound()
        {
            // Arrange.
            //a thumbnail published in two formats at the same width
            videoManifestMock.Setup(m => m.Metadata).Returns(new VideoManifestMetadataV2(
                "Title",
                "Description",
                420,
                [],
                new ThumbnailV2(1.77f, "LEHV6nWB2yk8pyo0adR*.7kCMdnj",
                [
                    new ImageSourceV2(480, "thumbs/480.jpg", "jpeg"),
                    new ImageSourceV2(480, "thumbs/480.webp", "webp")
                ]),
                1.77f,
                123456,
                null,
                null));
            SetupManifestFound();
            SetupVideoFound();

            // Action.
            var result = await manifestModel.OnGetAsync(manifestReference);

            // Assert.
            Assert.IsType<PageResult>(result);
            Assert.Equal("Title", manifestModel.VideoManifest.Title);
            Assert.Equal("Description", manifestModel.VideoManifest.Description);
            Assert.Equal(manifestReference, manifestModel.VideoManifest.ManifestReference);
            Assert.Equal("videoId", manifestModel.VideoManifest.VideoInfo?.VideoId);
        }

        [Fact]
        public async Task OnGetAsync_WhenManifestIsNotFound()
        {
            // Arrange.
            SetupManifestNotFound();

            // Action.
            var result = await manifestModel.OnGetAsync(manifestReference);

            // Assert.
            AssertRedirectToIndexSearch(result);
        }

        [Fact]
        public async Task OnPostForceNewValidationAsync_WhenManifestIsNotFound()
        {
            // Arrange.
            SetupManifestNotFound();

            // Action.
            var result = await manifestModel.OnPostForceNewValidationAsync(manifestReference);

            // Assert.
            AssertRedirectToIndexSearch(result);
            backgroundJobClientMock.Verify(c => c.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
        }

        [Fact]
        public async Task OnPostForceNewValidationAsync_WhenVideoIsFound()
        {
            // Arrange.
            SetupManifestFound();
            SetupVideoFound();

            // Action.
            var result = await manifestModel.OnPostForceNewValidationAsync(manifestReference);

            // Assert.
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Index", redirect.PageName);
            backgroundJobClientMock.Verify(c => c.Create(
                It.Is<Job>(j => j.Type == typeof(IVideoManifestValidatorTask) &&
                    "videoId".Equals(j.Args[0]) &&
                    manifestReference.ToString().Equals(j.Args[1])),
                It.Is<IState>(s => ((EnqueuedState)s).Queue == Queues.METADATA_VIDEO_VALIDATOR)),
                Times.Once);
        }

        [Fact]
        public async Task OnPostForceNewValidationAsync_WhenVideoIsNotFound()
        {
            // Arrange.
            SetupManifestFound();
            indexDbContextMock.Setup(c => c.Videos.TryFindOneAsync(It.IsAny<Expression<Func<Video, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Video?)null);

            // Action.
            var result = await manifestModel.OnPostForceNewValidationAsync(manifestReference);

            // Assert.
            //back to the manifest page: an orphan manifest can't be validated
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Null(redirect.PageName);
            Assert.Equal(manifestReference, Assert.IsType<SwarmReference>(redirect.RouteValues?["manifestReference"]));
            backgroundJobClientMock.Verify(c => c.Create(It.IsAny<Job>(), It.IsAny<IState>()), Times.Never);
        }

        // Helpers.
        private void AssertRedirectToIndexSearch(IActionResult result)
        {
            var redirect = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("Index", redirect.PageName);
            Assert.Equal(manifestReference, Assert.IsType<SwarmReference>(redirect.RouteValues?["manifestReference"]));
        }

        private void SetupManifestFound() =>
            indexDbContextMock.Setup(c => c.VideoManifests.TryFindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(videoManifestMock.Object);

        private void SetupManifestNotFound() =>
            indexDbContextMock.Setup(c => c.VideoManifests.TryFindOneAsync(It.IsAny<Expression<Func<VideoManifest, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((VideoManifest?)null);

        private void SetupVideoFound() =>
            indexDbContextMock.Setup(c => c.Videos.TryFindOneAsync(It.IsAny<Expression<Func<Video, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(videoMock.Object);
    }
}
