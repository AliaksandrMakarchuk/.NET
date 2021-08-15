using System;
using Microsoft.Extensions.Logging;

namespace WebRestApi
{
    /// <summary>
    /// 
    /// </summary>
    public static class LoggerExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId"></param>
        /// <param name="errorMessage"></param>
        /// <param name="ex"></param>
        public static void LogException(this ILogger logger, EventId eventId, string errorMessage, Exception ex)
        {
            var message = $"{errorMessage}.{Environment.NewLine}" +
            "Exception message: {ex.Message}{Environment.NewLine}" +
            "Exception StackTrace: {ex.StackTrace}";

            logger.LogError(eventId, message);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="eventId"></param>
        /// <param name="ex"></param>
        public static void LogException(this ILogger logger, EventId eventId, Exception ex)
        {
            var message = $"Exception message: {ex.Message}{Environment.NewLine}" +
            "Exception StackTrace: {ex.StackTrace}";

            logger.LogError(eventId, message);
        }
    }
}