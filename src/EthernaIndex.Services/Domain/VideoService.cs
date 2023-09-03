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
using System.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    internal sealed class VideoService : IVideoService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;

        // Constructor.
        public VideoService(IIndexDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        // Methods.
        public async Task DeleteVideoAsync(Video video)
        {
            // Delete manifests.
            foreach (var manifest in video.VideoManifests)
                await dbContext.VideoManifests.DeleteAsync(manifest);

            // Delete video.
            await dbContext.Videos.DeleteAsync(video);
        }

        public async Task ModerateUnsuitableVideoAsync(Video video, ManualVideoReview manualVideoReview)
        {
            // Delete unsuitable manifests.
            //save manifest list
            var videoManifests = video.VideoManifests.ToList();

            //set video as unsuitable
            video.SetAsUnsuitable(manualVideoReview);
            await dbContext.SaveChangesAsync();

            //remove all VideoManifests from database
            foreach (var videoManifest in videoManifests)
                await dbContext.VideoManifests.DeleteAsync(videoManifest);
        }
    }
}
