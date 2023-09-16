using Etherna.EthernaIndex.Domain.Models.UserAgg;
using Etherna.EthernaIndex.Domain.Models.VideoAgg;
using Etherna.EthernaIndex.Domain.Models;
using Etherna.EthernaIndex.ElasticSearch.Documents;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class Video2GenericManifestDto : Video2Base
    {
        // Constructors.
        public Video2GenericManifestDto(
            Video video,
            VideoManifest? lastValidManifest,
            UserSharedInfo ownerSharedInfo,
            VideoVote? currentUserVideoVote)
            : base(video, ownerSharedInfo, currentUserVideoVote)
        {
            if (lastValidManifest is not null)
                Manifest = new VideoManifest2Dto(lastValidManifest);
        }

        public Video2GenericManifestDto(
            VideoDocument videoDocument,
            UserSharedInfo ownerSharedInfo,
            VideoVote? currentUserVideoVote)
            : base(videoDocument, ownerSharedInfo, currentUserVideoVote)
        {
            Manifest = new VideoManifest2Dto(videoDocument);
        }

        // Properties.
        public VideoManifest2Dto? Manifest { get; }
    }
}
