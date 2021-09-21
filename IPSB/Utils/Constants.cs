using System.Collections.Generic;

namespace IPSB.Utils
{
    public class Constants
    {
        public static readonly int PAGE_SIZE = 50;

        public static readonly int MAXIMUM_PAGE_SIZE = 250;

        public static class Role
        {
            public const string ADMIN = "Admin";

            public const string STORE_OWNER = "Store Owner";

            public const string BUILDING_MANAGER = "Building Manager";

            public const string VISITOR = "Visitor";

            public static readonly string[] ROLE_LIST = { ADMIN, STORE_OWNER, BUILDING_MANAGER, VISITOR };
        }

        public static class TokenParams
        {
            public const int MINUTE_TO_EXPIRES = 30;
            public const int DAY_TO_EXPIRES = 15;
        }

        public static class Config
        {
            public const string ISSUER = "jwt:Issuer";
            public const string AUDIENCE = "jwt:Audience";
            public const string KEY = "jwt:Key";
        }

        public static class HeaderClaims
        {
            public const string FIREBASE_AUTH = "FirebaseAuth";
        }

        public static class AccountStatus
        {
            public const string ACTIVE = "ACTIVE";
        }

        public static class Status
        {
            public const string ACTIVE = "Active";

            public const string INACTIVE = "Inactive";

            public const string DISABLED = "Disabled";

            public const string NEW = "New";

            public const string MISSING = "Missing";

            public const string USED = "Used";
            
            public const string NOT_USED = "NotUsed";

            public const string DELETED = "Deleted";
        }

        public static class AppSetting
        {
            public const string FirebaseBucket = "Firebase:Bucket";
            public const string FirebaseApiKey = "Firebase:ApiKey";
            public const string FirebaseAuthEmail = "Firebase:Email";
            public const string FirebaseAuthPassword = "Firebase:Password";

        }

        public static class QueryKeys
        {
            public const string BUILDING_MANAGER_ID = "buildingManagerId";
        }
        
        public static class DefaultValue
        {
            public const int INTEGER = default;
            public const string STRING = default;
            public const float FLOAT = default;
            public const bool BOOLEAN = default;
        }

        public static class Request
        {
            public const string IF_MODIFIED_SINCE = "if-modified-since";
        }

        public static class Response
        {
            public const string LAST_MODIFIED = "last-modified";
        }
        
        public static class ExceptionMessage
        {
            public const string NOT_MODIFIED = "Not-modified";
        }


        
    }
}
