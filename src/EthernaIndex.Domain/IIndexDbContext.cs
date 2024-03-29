﻿//   Copyright 2021-present Etherna Sagl
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

using Etherna.DomainEvents;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Repositories;

namespace Etherna.EthernaIndex.Domain
{
    public interface IIndexDbContext : IDbContext
    {
        IRepository<Comment, string> Comments { get; }
        IRepository<ManualVideoReview, string> ManualVideoReviews { get; }
        IRepository<UnsuitableVideoReport, string> UnsuitableVideoReports { get; }
        IRepository<User, string> Users { get; }
        IRepository<VideoManifest, string> VideoManifests { get; }
        IRepository<Video, string> Videos { get; }
        IRepository<VideoVote, string> Votes { get; }

        IEventDispatcher EventDispatcher { get; }
    }
}
