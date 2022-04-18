namespace Etherna.EthernaIndex.Domain.Models.VideoAgg
{
    public enum ValidationErrorType
    {
        InvalidVideoSource,
        InvalidThumbnailSource,
        JsonConvert,
        MissingDuration,
        MissingOriginalQuality,
        MissingTitle,
        Unknown,
    }
}
