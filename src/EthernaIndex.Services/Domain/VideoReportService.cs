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
using Etherna.MongoDB.Driver.Linq;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    public class VideoReportService : IVideoReportService
    {
        // Fields.
        private readonly IIndexContext dbContext;

        // Constructor.
        public VideoReportService(IIndexContext dbContext)
        {
            this.dbContext = dbContext;
        }


        public async Task ApproveVideoAsync(string hashReportVideo)
        {
            await ModerateVideoAsync(hashReportVideo, true);
        }

        public async Task RejectVideoAsync(string hashReportVideo)
        {
            await ModerateVideoAsync(hashReportVideo, false);
        }

        private async Task ModerateVideoAsync(string hashReportVideo, bool isApproved)
        {
            var videoReports = await dbContext.VideoReports.QueryElementsAsync(elements =>
                elements.Where(u => u.Video.ManifestHash.Hash == hashReportVideo &&
                                    u.LastCheck == null) //Only Report to check
                        .ToCursorAsync());

            while (await videoReports.MoveNextAsync())
            {
                foreach (var item in videoReports.Current)
                {
                    if (isApproved)
                    {
                        item.ApproveContent();
                    }
                    else
                    {
                        item.RejectContent();
                    }
                }
            }

            if (!isApproved)
            {
                var video = await dbContext.Videos.TryFindOneAsync(u => u.ManifestHash.Hash == hashReportVideo);
                if (video is not null)
                    await dbContext.Videos.DeleteAsync(video);
                //TODO need to remove VideoReport, VideoVote, ManifestMetadata?
            }
        }
    }
}
