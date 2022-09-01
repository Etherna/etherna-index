﻿using Etherna.EthernaIndex.Domain.Models;

namespace Etherna.EthernaIndex.Areas.Api.DtoModels
{
    public class SystemParametersDto
    {
        public int CommentMaxLength => Comment.MaxLength;
        public int VideoDescriptionMaxLength => Video.DescriptionMaxLength;
        public int VideoTitleMaxLength => Video.TitleMaxLength;
    }
}
