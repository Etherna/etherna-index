﻿using Etherna.MongODM.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.Attributes
{
    public class SimpleExceptionFilterAttribute : ExceptionFilterAttribute
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
                case InvalidEntityTypeException _:
                    context.Result = new BadRequestObjectResult(context.Exception.Message);
                    break;
                case EntityNotFoundException _:
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