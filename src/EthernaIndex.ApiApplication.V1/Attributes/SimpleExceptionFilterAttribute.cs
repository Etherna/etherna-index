using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;

namespace Etherna.EthernaIndex.ApiApplication.V1.Attributes
{
    public class SimpleExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            switch (context.Exception)
            {
                case ArgumentException _:
                case FormatException _:
                    context.Result = new BadRequestObjectResult(context.Exception.Message);
                    break;
                case KeyNotFoundException _:
                    context.Result = new NotFoundObjectResult(context.Exception.Message);
                    break;
                case InvalidOperationException _:
                case UnauthorizedAccessException _:
                    context.Result = new UnauthorizedResult();
                    break;
            }
        }
    }
}
