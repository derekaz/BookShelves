//using BookShelves.Maui.Services;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.HttpLogging;

//namespace BookShelves.Maui.Services;

//public class CustomLoggingInterceptor : IHttpLoggingInterceptor
//{
//    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext context)
//    {
//        // Example: Remove specific headers from being logged
//        context.HttpContext.Request.Headers.Remove("X-API-Key");

//        // Example: Add custom information to log
//        context.AddParameter("RequestId", Guid.NewGuid().ToString());

//        return ValueTask.CompletedTask;
//    }

//    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext context)
//    {
//        // Example: Remove sensitive response header
//        logContext.HttpContext.Response.Headers.Remove("Set-Cookie");

//        // Example: Log additional context
//        context.AddParameter("new-response-field", Guid.NewGuid().ToString());

//        return ValueTask.CompletedTask;
//    }
//}