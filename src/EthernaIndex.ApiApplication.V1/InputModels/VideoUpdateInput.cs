using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.ApiApplication.V1.InputModels
{
    public class VideoUpdateInput
    {
        public string Description { get; set; }
        public string ThumbnailHash { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
