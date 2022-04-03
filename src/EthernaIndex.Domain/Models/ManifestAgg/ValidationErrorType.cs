namespace Etherna.EthernaIndex.Domain.Models.ManifestAgg
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
