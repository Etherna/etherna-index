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
    /// Drops the Elasticsearch indexes and recreates them from scratch, then reindexes all documents.
    /// Use it when the index structure has to change (mapping/settings migrations) or to recover from a
    /// corrupted/drifted index: it guarantees a pristine rebuild, but the indexes are unavailable while
    /// it runs (downtime), so it is a maintenance-window operation.
    /// For routine orphan reconciliation without downtime use <see cref="IReindexElasticDocumentsTask"/>.
    /// </summary>
    public interface IRebuildElasticIndexesTask
    {
        [Queue(Queues.ELASTIC_SEARCH_MAINTENANCE)]
        Task RunAsync();
    }
}
