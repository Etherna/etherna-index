namespace Etherna.EthernaIndex.ElasticSearch.DtoModel
{
    public class FindVideoDto
    {
        // Properties.
        public string? Description { get; set; }
        public FilterType FilterType { get; set; }
        public string? Title { get; set; }
        public int Take { get; set; }
        public int Page { get; set; }
    }
}
