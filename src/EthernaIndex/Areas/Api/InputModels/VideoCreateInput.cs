using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.Areas.Api.InputModels
{
    public class VideoCreateInput
    {
        public string Description { get; set; } = default!;
        public int LengthInSeconds { get; set; }
        public string ThumbnailHash { get; set; } = default!;
        [Required]
        public string Title { get; set; } = default!;
        [Required]
        public string VideoHash { get; set; } = default!;
    }
}
