using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.Areas.Api.InputModels
{
    public class ChannelCreateInput
    {
        [Required]
        public string Address { get; set; } = default!;
    }
}
