using System;
using System.Net;
using BlogLab.Models.Exception;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace BlogLab.Web.Extensions
{
    public static class ExceptionMiddlewareExtensions
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
               appError.Run(async context => 
               {
                   context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                   context.Response.ContentType = "application/json";
                   var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                   if (contextFeature != null)
                   {
                       // in production we would log exceptions into the database
                       await context.Response.WriteAsync(new ApiException()
                       {
                           StatusCode = context.Response.StatusCode,
                           Message = "Internal Server Error"
                       }.ToString());
                   }
               });
            });
        }
    }
}
