using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.ApiApplication.InputModels
{
    public class VideoUpdateInput
    {
        public string Description { get; set; }
        public string ThumbnailHash { get; set; }
        [Required]
        public string Title { get; set; }
    }
}
