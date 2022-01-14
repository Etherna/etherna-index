using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Exceptions
{
    public class DuplicatedManifestHashException : Exception
    {
        public DuplicatedManifestHashException(string manifestHash) :
            base ($"hash {manifestHash} is duplicated")
        {

        }

        public DuplicatedManifestHashException()
        {

        }

        public DuplicatedManifestHashException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
