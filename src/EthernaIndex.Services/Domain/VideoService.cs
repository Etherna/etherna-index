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

        public async Task ModerateUnsuitableVideoAsync(Video video)
        {
            // Delete unsuitable manifests.
            //save manifest list
            var videoManifests = video.VideoManifests.ToList();

            //set video as unsuitable
            video.SetAsUnsuitable();
            await dbContext.SaveChangesAsync();

            //remove all VideoManifests from database
            foreach (var videoManifest in videoManifests)
                await dbContext.VideoManifests.DeleteAsync(videoManifest);
        }
    }
}
