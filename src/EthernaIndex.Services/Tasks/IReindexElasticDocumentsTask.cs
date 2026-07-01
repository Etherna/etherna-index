// Copyright 2021-present Etherna SA
// This file is part of Etherna Index.
// 
// Etherna Index is free software: you can redistribute it and/or modify it under the terms of the
// GNU Affero General Public License as published by the Free Software Foundation,
// either version 3 of the License, or (at your option) any later version.
// 
// Etherna Index is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
// without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License along with Etherna Index.
// If not, see <https://www.gnu.org/licenses/>.

using Hangfire;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Services.Tasks
{
    /// <summary>
    /// Reindexes all documents in place (existing indexes are kept) and then prunes orphan documents,
    /// i.e. documents still indexed but no longer present in the primary store. Search stays available
    /// throughout (zero-downtime), so this is the operation for routine reconciliation.
    /// It does not apply index structure changes: for mapping/settings migrations or to rebuild a
    /// corrupted index use <see cref="IRebuildElasticIndexesTask"/>.
    /// </summary>
    public interface IReindexElasticDocumentsTask
    {
        [Queue(Queues.ELASTIC_SEARCH_MAINTENANCE)]
        Task RunAsync();
    }
}