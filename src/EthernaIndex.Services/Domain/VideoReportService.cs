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

using Etherna.EthernaIndex.Domain;
using Etherna.EthernaIndex.Domain.Models;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    public class VideoReportService : IVideoReportService
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public VideoReportService(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Methods.
        public async Task SetReviewAsync(string videoId, string hashReportVideo, ContentReviewType contentReview, User user)
        {
            if (hashReportVideo is null)
                throw new ArgumentNullException(nameof(hashReportVideo));
            if (videoId is null)
                throw new ArgumentNullException(nameof(videoId));

            var video = await indexDbContext.Videos.FindOneAsync(i => i.Id == videoId);
            if (contentReview == ContentReviewType.ApprovedManifest ||
                contentReview == ContentReviewType.RejectManifest)
            {
                if (video.GetLastValidManifest()?.Title == hashReportVideo) //Set review only for last valid hashReportVideo
                    video.SetReview(contentReview);
                else
                    return;
            }
            else
                video.SetReview(contentReview);

            var review = await indexDbContext.VideoReviews.TryFindOneAsync(i => i.Video.Id == videoId &&
                                                                            i.ManifestHash == hashReportVideo);
            if (review is null)
            {
                review = new VideoReview(contentReview, "", hashReportVideo, user, video);
                await indexDbContext.VideoReviews.CreateAsync(review);
            }
            else
            {
                review.ChangeReview(contentReview, "");
            }
        }
    }
}
