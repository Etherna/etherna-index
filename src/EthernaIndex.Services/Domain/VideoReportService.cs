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
using System.Linq;
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
        public async Task SetReviewAsync(
            string videoId, 
            string hashReportVideo, 
            ContentReviewStatus contentReview, 
            User reviewUser, 
            string description)
        {
            if (hashReportVideo is null)
                throw new ArgumentNullException(nameof(hashReportVideo));
            if (videoId is null)
                throw new ArgumentNullException(nameof(videoId));
            if (reviewUser is null)
                throw new ArgumentNullException(nameof(reviewUser));

            // Get video and manifest data.
            var video = await indexDbContext.Videos.FindOneAsync(v => v.Id == videoId);
            var videoManifest = await indexDbContext.VideoManifests.FindOneAsync(vm => vm.ManifestHash.Hash == hashReportVideo);

            if (videoManifest.Video.Id != video.Id)
            {
                var ex = new InvalidOperationException("Missmatching between manifest and video");
                ex.Data.Add("VideoId", video.Id);
                ex.Data.Add("VideoManifests.Hash", videoManifest.ManifestHash.Hash);
                ex.Data.Add("VideoManifests.Id", videoManifest.Video.Id);
                throw ex;
            }

            if (contentReview == ContentReviewStatus.RejectVideo)
            {
                await indexDbContext.Videos.DeleteAsync(video);
                await indexDbContext.VideoManifests.DeleteAsync(videoManifest);
            }
            else if (contentReview == ContentReviewStatus.RejectManifest)
            {
                var hasOtherValidManifest = video.VideoManifests.Any(vm => vm.IsValid == true &&
                                                                    vm.ManifestHash.Hash != videoManifest.ManifestHash.Hash);
                if (!hasOtherValidManifest)
                {
                    var ex = new InvalidOperationException("RejectManifest only when there are other valid manifest for video");
                    ex.Data.Add("VideoId", video.Id);
                    ex.Data.Add("VideoManifests.Hash", videoManifest.ManifestHash.Hash);
                    throw ex;
                }

                videoManifest.SetReviewRejected(contentReview); //TODO for review SetReviewStatus is mandatory for call RemoveManifest
                video.RemoveManifest(videoManifest);
                await indexDbContext.VideoManifests.DeleteAsync(videoManifest);
            }
            else 
                videoManifest.SetReviewApproved(contentReview);

            var videoReview = new VideoReview(contentReview, description, videoManifest.ManifestHash.Hash, reviewUser, video.Id);
            await indexDbContext.VideoReviews.CreateAsync(videoReview);
        }
    }
}
