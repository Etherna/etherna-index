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
using Etherna.MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    public class VideoReportService : IVideoReportService
    {
        // Fields.
        private readonly IIndexDbContext dbContext;

        // Constructor.
        public VideoReportService(IIndexDbContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task ApproveAsync(string hashReportVideo, bool onlyManifest)
        {
            await ManageReportAsync(hashReportVideo, true, onlyManifest);
        }

        public async Task RejectAsync(string hashReportVideo, bool onlyManifest)
        {
            await ManageReportAsync(hashReportVideo, false, onlyManifest);
        }

        // Helpers.
        private void ApproveContent(VideoReport videoReport, bool onlyManifest)
        {
            if (videoReport is null)
                return;

            videoReport.ApproveContent(onlyManifest);
        }

        private async Task ManageReportAsync(string hashReportVideo, bool isApproved, bool onlyManifest)
        {
            var videoReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.VideoManifest.ManifestHash.Hash == hashReportVideo &&
                                    u.LastCheck == null) //Only Report to check
                        .ToCursorAsync());

            while (await videoReports.MoveNextAsync())
                foreach (var item in videoReports.Current)
                    if (isApproved)
                        ApproveContent(item, onlyManifest);
                    else
                        await RejectContentAsync(item, onlyManifest);
        }

        private async Task RejectContentAsync(VideoReport videoReport, bool onlyManifest)
        {
            if (videoReport is null)
                return;

            videoReport.RejectContent(onlyManifest);

            var video = await dbContext.VideoReports.TryFindOneAsync(u => u.Id == videoReport.Id);
            if (video is not null)
                await dbContext.Videos.DeleteAsync(video);
            //TODO need to remove VideoReport, VideoVote, ManifestMetadata?
        }
    }
}
