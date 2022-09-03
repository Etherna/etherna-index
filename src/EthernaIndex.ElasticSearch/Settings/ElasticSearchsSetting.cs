using System.Collections.Generic;

namespace Etherna.EthernaIndex.ElasticSearch.Settings
{
    public class ElasticSearchsSetting
    {
        public IReadOnlyCollection<string> Urls { get; set; } = default!;
    }
}
