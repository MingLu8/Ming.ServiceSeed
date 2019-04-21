using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ServiceSeed
{
    public class ScopedLoggingMiddleware
    {
        const string CORRELATION_ID_HEADER_NAME = "CorrelationID";
        private readonly RequestDelegate next;
        private readonly ILogger<ScopedLoggingMiddleware> logger;

        public ScopedLoggingMiddleware(RequestDelegate next, ILogger<ScopedLoggingMiddleware> logger)
        {
            this.next = next ?? throw new System.ArgumentNullException(nameof(next));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task Invoke(HttpContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            var correlationId = GetOrAddCorrelationHeader(context);

            try
            {
                var loggerState = new LoggerState
                {
                    [CORRELATION_ID_HEADER_NAME] = correlationId
                    //Add any number of properties to be logged under a single scope
                };

                using (logger.BeginScope(loggerState))
                {
                    await next(context);
                }
            }
            //To make sure that we don't loose the scope in case of an unexpected error
            catch (Exception ex) when (LogOnUnexpectedError(ex))
            {
            }
        }

        private string GetOrAddCorrelationHeader(HttpContext context)
        {
            if (context == null) throw new System.ArgumentNullException(nameof(context));

            if (string.IsNullOrWhiteSpace(context.Request.Headers[CORRELATION_ID_HEADER_NAME]))
                context.Request.Headers[CORRELATION_ID_HEADER_NAME] = Guid.NewGuid().ToString();

            return context.Request.Headers[CORRELATION_ID_HEADER_NAME];
        }

        private bool LogOnUnexpectedError(Exception ex)
        {
            logger.LogError(ex, "An unexpected exception occured!");
            return true;
        }
    }

    internal class LoggerState : Dictionary<string, object>
    {
        const string seperator = " => ";
        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var item in this)
                builder.Append($"{item.Key}:{item.Value}{seperator}");

            return builder.ToString().Replace(seperator, string.Empty);
        }
    }
}