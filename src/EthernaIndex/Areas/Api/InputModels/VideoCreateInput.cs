using Etherna.EthernaIndex.Domain.Models;
using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.Areas.Api.InputModels
{
    public class VideoCreateInput
    {
        public string? EncryptionKey { get; set; }
        public EncryptionType EncryptionType { get; set; }
        [Required]
        public string ManifestHash { get; set; } = default!;
    }
}
