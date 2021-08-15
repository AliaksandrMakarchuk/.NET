using Microsoft.Extensions.Logging;

namespace WebRestApi.Service.Models
{
    public static partial class LoggingEvents
    {
        private enum MESSAGEIDS{
            GETALL,
            GETBYID,
            GETBYUSER,
            CREATEMESSAGE,
            DELETEALL,
            DELETEMESSAGE,
            DELETEMESSAGEBYID,
            DELETEMESSAGEBYUSER,
            UPDATE
        };

        public static EventId GetAllMessages => GetMessageAreaEventId((int)MESSAGEIDS.GETALL);
        public static EventId GetMessageById => GetMessageAreaEventId((int)MESSAGEIDS.GETBYID);
        public static EventId GetMessagesByUser => GetMessageAreaEventId((int)MESSAGEIDS.GETBYUSER);
        public static EventId CreateMessage => GetMessageAreaEventId((int)MESSAGEIDS.CREATEMESSAGE);
        public static EventId DeleteAllMessages => GetMessageAreaEventId((int)MESSAGEIDS.DELETEALL);
        public static EventId DeleteMessage => GetMessageAreaEventId((int)MESSAGEIDS.DELETEMESSAGE);
        public static EventId DeleteMessageById => GetMessageAreaEventId((int)MESSAGEIDS.DELETEMESSAGEBYID);
        public static EventId DeleteMessageByUser => GetMessageAreaEventId((int)MESSAGEIDS.DELETEMESSAGEBYUSER);
        public static EventId UpdateMessage => GetMessageAreaEventId((int)MESSAGEIDS.UPDATE);

        // Error area
        public static EventId WrongMessageIdentificator => GetMessageErrorAreaEventId((int)MESSAGEIDS.GETBYID);
        public static EventId ErrorOnGetAllMessages => GetMessageErrorAreaEventId((int)MESSAGEIDS.GETALL);
        public static EventId ErrorOnGetAllMessageByUser => GetMessageErrorAreaEventId((int)MESSAGEIDS.GETBYUSER);
        public static EventId ErrorOnGetMessageById => GetMessageErrorAreaEventId((int)MESSAGEIDS.GETBYID);
        public static EventId ErrorOnCreateMessage => GetMessageErrorAreaEventId((int)MESSAGEIDS.CREATEMESSAGE);
        public static EventId ErrorOnDeleteAllMessages => GetMessageErrorAreaEventId((int)MESSAGEIDS.DELETEALL);
        public static EventId ErrorOnDeleteMessage => GetMessageErrorAreaEventId((int)MESSAGEIDS.DELETEMESSAGE);
        public static EventId ErrorOnDeleteMessageById => GetMessageErrorAreaEventId((int)MESSAGEIDS.DELETEMESSAGEBYID);
        public static EventId ErrorOnDeleteMessageByUser => GetMessageErrorAreaEventId((int)MESSAGEIDS.DELETEMESSAGEBYUSER);
        public static EventId ErrorOnUpdateMessage => GetMessageErrorAreaEventId((int)MESSAGEIDS.UPDATE);
    }
}