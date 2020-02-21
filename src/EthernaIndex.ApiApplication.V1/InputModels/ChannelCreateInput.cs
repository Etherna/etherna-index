using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.ApiApplication.V1.InputModels
{
    public class ChannelCreateInput
    {
        [Required]
        public string Address { get; set; }
    }
}
