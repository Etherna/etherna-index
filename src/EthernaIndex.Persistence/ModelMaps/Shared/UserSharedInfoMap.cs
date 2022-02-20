﻿using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.MongODM.Core;
using Etherna.MongODM.Core.Serialization;

namespace Etherna.EthernaIndex.Persistence.ModelMaps.Shared
{
    class UserSharedInfoMap : IModelMapsCollector
    {
        public void Register(IDbContext dbContext)
        {
            dbContext.SchemaRegistry.AddModelMapsSchema<UserSharedInfo>("6d0d2ee1-6aa3-42ea-9833-ac592bfc6613", mm =>
            {
                mm.AutoMap();

                // Set members to ignore if null or default.
                mm.GetMemberMap(u => u.LockoutEnd).SetIgnoreIfNull(true);
            });
        }
    }
}
