using Digicando.MongODM.Models;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Domain.Models
{
    public abstract class ModelBase : IModel
    {
        public virtual IDictionary<string, object> ExtraElements { get; protected set; } =
            new Dictionary<string, object>();
    }
}
