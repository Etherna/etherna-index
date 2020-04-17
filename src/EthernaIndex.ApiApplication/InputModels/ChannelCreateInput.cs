using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.ApiApplication.InputModels
{
    public class ChannelCreateInput
    {
        [Required]
        public string Address { get; set; }
    }
}
