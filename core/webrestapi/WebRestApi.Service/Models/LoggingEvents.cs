using Microsoft.Extensions.Logging;

namespace WebRestApi.Service.Models
{
    public static partial class LoggingEvents
    {
        private const int TOKENEVENTAREA = 10;
        private const int USEREVENTAREA = 100;
        private const int MESSAGEEVENTAREA = 200;
        private const int ERROREVENTAREA = 1000;

        private static EventId GetTokenAreaEventId(int id)
        {
            return new EventId(TOKENEVENTAREA + id);
        }

        private static EventId GetUserAreaEventId(int id)
        {
            return new EventId(USEREVENTAREA + id);
        }

        private static EventId GetMessageAreaEventId(int id)
        {
            return new EventId(MESSAGEEVENTAREA + id);
        }

        private static EventId GetTokenErrorAreaEventId(int id)
        {
            return new EventId(ERROREVENTAREA + TOKENEVENTAREA + id);
        }

        private static EventId GetUserErrorAreaEventId(int id)
        {
            return new EventId(ERROREVENTAREA + USEREVENTAREA + id);
        }

        private static EventId GetMessageErrorAreaEventId(int id)
        {
            return new EventId(ERROREVENTAREA + MESSAGEEVENTAREA + id);
        }
    }
}