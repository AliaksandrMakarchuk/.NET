using Microsoft.Extensions.Logging;

namespace WebRestApi.Service.Models
{
    public static class LoggingEvents
    {
        public static EventId GetAllUsers => new EventId(0);
        public static EventId GetUserById => new EventId(1);
        public static EventId GetUsersByName => new EventId(2);
        public static EventId DeleteUser => new EventId(3);
        public static EventId UpdateUserName => new EventId(4);
        public static EventId SendMessage => new EventId(5);
        public static EventId GetMessageByUser => new EventId(6);
        public static EventId AuthenticationPingRequest => new EventId(7);
        public static EventId AuthenticationGetToken => new EventId(8);
        public static EventId AuthenticationGetIdentity => new EventId(9);

        public static EventId GetUsersNotFound => new EventId(100);
        public static EventId WrongUserIdentifier => new EventId(101);
        public static EventId ErrorOnSavingChanges => new EventId(102);
        public static EventId ErrorOnDeletingUser => new EventId(103);
        public static EventId ErrorOnUpdateUserName => new EventId(104);
        public static EventId ErrorOnSendMessage => new EventId(105);
        public static EventId ErrorOnGettingMessageByUser => new EventId(106);
    }
}