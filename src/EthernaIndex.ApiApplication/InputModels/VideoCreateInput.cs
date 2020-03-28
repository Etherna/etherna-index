using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.ApiApplication.InputModels
{
    public class VideoCreateInput
    {
        public string Description { get; set; }
        public int LengthInSeconds { get; set; }
        public string ThumbnailHash { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string VideoHash { get; set; }
    }
}
