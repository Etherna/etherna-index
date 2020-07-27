﻿using System.ComponentModel.DataAnnotations;

namespace Etherna.EthernaIndex.Areas.Api.InputModels
{
    public class VideoUpdateInput
    {
        public string Description { get; set; } = default!;
        public string ThumbnailHash { get; set; } = default!;
        [Required]
        public string Title { get; set; } = default!;
    }
}