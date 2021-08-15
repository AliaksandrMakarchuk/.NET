using Microsoft.Extensions.Logging;

namespace WebRestApi.Service.Models
{
    public static partial class LoggingEvents
    {
        private enum USERIDS
        {
            GETALL,
            GETBYID,
            GETBYNAME,
            CREATEUSER,
            DELETEALL,
            DELETEUSER,
            DELETEBYID,
            UPDATEUSERNAME
        };

        public static EventId GetAllUsers => GetUserAreaEventId((int)USERIDS.GETALL);
        public static EventId GetUserById => GetUserAreaEventId((int)USERIDS.GETBYID);
        public static EventId GetUsersByName => GetUserAreaEventId((int)USERIDS.GETBYNAME);
        public static EventId CreateUser => GetUserAreaEventId((int)USERIDS.CREATEUSER);
        public static EventId DeleteAllUsers => GetUserAreaEventId((int)USERIDS.DELETEALL);
        public static EventId DeleteUser => GetUserAreaEventId((int)USERIDS.DELETEUSER);
        public static EventId DeleteUserById => GetUserAreaEventId((int)USERIDS.DELETEBYID);
        public static EventId UpdateUserName => GetUserAreaEventId((int)USERIDS.UPDATEUSERNAME);

        // Error area
        public static EventId WrongUserIdentifier => GetUserErrorAreaEventId((int)USERIDS.GETBYID);
        public static EventId ErrorOnGetAllUsers => GetUserErrorAreaEventId((int)USERIDS.GETALL);
        public static EventId ErrorOnGetUserById => GetUserErrorAreaEventId((int)USERIDS.GETBYID);
        public static EventId ErrorOnCreateUser => GetUserErrorAreaEventId((int)USERIDS.CREATEUSER);
        public static EventId ErrorOnDeleteAllUsers => GetUserErrorAreaEventId((int)USERIDS.DELETEALL);
        public static EventId ErrorOnDeleteUser => GetUserErrorAreaEventId((int)USERIDS.DELETEUSER);
        public static EventId ErrorOnDeleteUserById => GetUserErrorAreaEventId((int)USERIDS.DELETEBYID);
        public static EventId ErrorOnUpdateUserName => GetUserErrorAreaEventId((int)USERIDS.UPDATEUSERNAME);
    }
}