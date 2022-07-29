using Etherna.EthernaIndex.Domain.Models;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Domain
{
    public interface IVideoService
    {
        /// <summary>
        /// Remove video and all its traces
        /// </summary>
        /// <param name="video">The video</param>
        Task DeleteVideoAsync(Video video);

        /// <summary>
        /// Remove video manifests, and keep video model as a trace
        /// </summary>
        /// <param name="video">The video</param>
        Task ModerateUnsuitableVideoAsync(Video video);
    }
}