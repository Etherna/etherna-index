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

using Etherna.BeeNet.Models;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace Etherna.EthernaIndex.Configs.Swagger.SchemaFilters
{
    public class SwarmHashSchemaFilter : ISchemaFilter
    {
        public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
        {
            ArgumentNullException.ThrowIfNull(schema);
            ArgumentNullException.ThrowIfNull(context);
            
            var concreteSchema = (OpenApiSchema)schema;
            if (context.Type == typeof(SwarmHash))
            {
                concreteSchema.Type = JsonSchemaType.String;
                concreteSchema.Format = null;
                concreteSchema.MinLength = SwarmHash.HashSize * 2;
                concreteSchema.MaxLength = SwarmHash.HashSize * 2;
                concreteSchema.Pattern = $"^[a-fA-F0-9]{{{SwarmHash.HashSize * 2}}}$";
            }
        }
    }
}