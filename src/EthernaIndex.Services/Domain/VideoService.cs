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
    public class VideoService : IVideoService
    {
        // Fields.
        private readonly IIndexDbContext indexDbContext;

        // Constructor.
        public VideoService(
            IIndexDbContext indexDbContext)
        {
            this.indexDbContext = indexDbContext;
        }

        // Methods.
        public async Task CreateManualReviewAsync(
            ManualVideoReview videoReview)
        {
            if (videoReview is null)
                throw new ArgumentNullException(nameof(videoReview));

            // Create review record.
            await indexDbContext.ManualVideoReviews.CreateAsync(videoReview);

            // Process result.
            if (!videoReview.IsValid)
                await indexDbContext.Videos.DeleteAsync(videoReview.VideoId);
        }
    }
}
