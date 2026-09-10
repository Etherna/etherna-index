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

using Etherna.MongoDB.Bson;
using Etherna.MongoDB.Driver;
using Etherna.MongoDB.Driver.Core.Clusters;
using Etherna.MongoDB.Driver.Core.Connections;
using Etherna.MongoDB.Driver.Core.Servers;
using Microsoft.AspNetCore.Http;
using System;
using System.Net;
using System.Threading.Tasks;
using Xunit;

namespace Etherna.EthernaIndex.Areas.Api
{
    public class ExceptionHandlerTest
    {
        // Methods.
        [Fact]
        public async Task MapTransientTransactionErrorToConflict()
        {
            // A write conflict between concurrent transactions: the server asks the client to retry.
            var exception = BuildWriteConflictException();
            exception.AddErrorLabel("TransientTransactionError");

            var result = await ExceptionHandler.RunAsync(() => throw exception);

            Assert.Equal(StatusCodes.Status409Conflict, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }

        [Fact]
        public async Task MapUnlabeledDriverExceptionToInternalServerError()
        {
            // The same driver exception without the transient label is not a retryable conflict.
            var exception = BuildWriteConflictException();

            var result = await ExceptionHandler.RunAsync(() => throw exception);

            Assert.Equal(StatusCodes.Status500InternalServerError, Assert.IsAssignableFrom<IStatusCodeHttpResult>(result).StatusCode);
        }

        // Helpers.
        private static MongoCommandException BuildWriteConflictException()
        {
            var connectionId = new ConnectionId(new ServerId(new ClusterId(), new DnsEndPoint("localhost", 27017)));
            var command = new BsonDocument("findAndModify", "videos");
            var result = new BsonDocument
            {
                { "ok", 0 },
                { "errmsg", "Write conflict during plan execution and yielding is disabled" },
                { "code", 112 },
                { "codeName", "WriteConflict" }
            };
            return new MongoCommandException(connectionId, "Command findAndModify failed", command, result);
        }
    }
}
