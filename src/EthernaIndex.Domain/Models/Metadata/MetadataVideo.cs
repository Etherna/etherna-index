using Etherna.EthernaIndex.Domain.DtoModel;
using Etherna.EthernaIndex.Domain.Interfaces;
using Etherna.MongODM.Core.Attributes;
using System;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Domain.Models.Meta
{
    public class MetadataVideo : EntityModelBase<string>
    {
        // Constructors.
        public static async Task<MetadataVideo> CreateMetadataVideoAsync(
            MetadataVideoDto metadataVideoDto,
            IMetadataVideoValidator metadataVideoValidator)
        {
            if (metadataVideoDto is null)
            {
                throw new ArgumentNullException(nameof(metadataVideoDto));
            }
            var metadataVideo = new MetadataVideo
            {
                Title = metadataVideoDto.Title ?? throw new ArgumentNullException(nameof(metadataVideoDto.Title), "All metadata video must have a Title."),
                Description = metadataVideoDto.Description ?? throw new ArgumentNullException(nameof(metadataVideoDto.Description), "All metadata video must have a Description."),
                OriginalQuality = metadataVideoDto.OriginalQuality ?? throw new ArgumentNullException(nameof(metadataVideoDto.OriginalQuality), "All metadata video must have a OriginalQuality."),
                OwnerAddress = metadataVideoDto.OwnerAddress ?? throw new ArgumentNullException(nameof(metadataVideoDto.OwnerAddress), "All metadata video must have a OwnerAddress."),
                Duration = metadataVideoDto.Duration
            };
            await metadataVideo.CheckValidationAsync(metadataVideoValidator);

            return metadataVideo;
        }
        protected MetadataVideo() { }


        // Properties.
        public string Title { get; init; } = default!;
        public string Description { get; init; } = default!;
        public string OriginalQuality { get; init; } = default!;
        public string OwnerAddress { get; init; } = default!;
        public int Duration { get; init; }
        public MetadataVideoValidationResult Status { get; set; }


        // Methods.
        [PropertyAlterer(nameof(Status))]
        public virtual async Task<MetadataVideoValidationResult> CheckValidationAsync(IMetadataVideoValidator metadataVideoValidator)
        {
            if (metadataVideoValidator is not null)
            {
                await metadataVideoValidator.CheckVideoFormatAsync(this);
            }

            Status = MetadataVideoValidationResult.Valid;

            return Status;
        }
    }
}
