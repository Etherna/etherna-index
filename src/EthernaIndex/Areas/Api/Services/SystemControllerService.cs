//   Copyright 2021-present Etherna Sagl
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using Etherna.EthernaIndex.Areas.Api.DtoModels;
using Etherna.EthernaIndex.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace Etherna.EthernaIndex.Areas.Api.Services
{
    internal class SystemControllerService : ISystemControllerService
    {
        // Fields.
        private readonly IConfiguration configuration;
        private readonly IIndexDbContext indexContext;

        // Constructors.
        public SystemControllerService(
            IConfiguration configuration,
            IIndexDbContext indexContext)
        {
            this.configuration = configuration;
            this.indexContext = indexContext;
        }

        // Methods.
        public Task<SettingsDto> GetSettingsAsync() =>
            Task.FromResult(new SettingsDto(
                configuration["Swarm:GatewayUrl"],
                "0.2"));

        public async Task<IActionResult> MigrateDatabaseAsync()
        {
            await indexContext.DbMigrationManager.StartDbContextMigrationAsync();
            return new OkResult();
        }
    }
}
