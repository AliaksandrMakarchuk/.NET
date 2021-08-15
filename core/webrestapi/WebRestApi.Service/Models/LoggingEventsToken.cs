using Microsoft.Extensions.Logging;

namespace WebRestApi.Service.Models
{
    public static partial class LoggingEvents
    {
        private enum TOKENIDS
        {
            PING,
            GETTOKEN,
            GETIDENTITY
        };

        public static EventId AuthenticationPingRequest => GetTokenAreaEventId((int)TOKENIDS.PING);
        public static EventId AuthenticationGetToken => GetTokenAreaEventId((int)TOKENIDS.GETTOKEN);
        public static EventId AuthenticationGetIdentity => GetTokenAreaEventId((int)TOKENIDS.GETIDENTITY);

        // Error area
        public static EventId ErrorOnGetToken => GetTokenErrorAreaEventId((int) TOKENIDS.GETTOKEN);
    }
}