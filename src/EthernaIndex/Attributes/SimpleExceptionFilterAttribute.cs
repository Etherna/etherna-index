﻿//   Copyright 2021-present Etherna Sagl
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

using Etherna.MongODM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Attributes
{
    public sealed class SimpleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context is null)
                throw new ArgumentNullException(nameof(context));

            switch (context.Exception)
            {
                case ArgumentException _:
                case FormatException _:
                case InvalidOperationException _:
                case MongodmInvalidEntityTypeException _:
                    context.Result = new BadRequestObjectResult(context.Exception.Message);
                    break;
                case MongodmEntityNotFoundException _:
                case KeyNotFoundException _:
                    context.Result = new NotFoundObjectResult(context.Exception.Message);
                    break;
                case UnauthorizedAccessException _:
                    context.Result = new UnauthorizedResult();
                    break;
            }
        }
    }
}
