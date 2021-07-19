using System.Collections.Generic;

namespace IPSB.Utils
{
    public class Constants
    {
        public static readonly int PAGE_SIZE = 50;

        public static readonly int MAXIMUM_PAGE_SIZE = 250;

        public static readonly string EXPIRES_IN_DAY = "86400";

        public static class Role
        {
            public const string ADMIN = "Admin";
            public const string STORE_OWNER = "Store Owner";
            public const string BUILDING_MANAGER = "Building Manager";
            public const string VISITOR = "Visitor";

            public static readonly string[] ROLE_LIST = { ADMIN, STORE_OWNER, BUILDING_MANAGER, VISITOR };
        }

        public static class PrefixPolicy
        {
            public const string REQUIRED_ROLE = "RequiredRole";
        }

        public static class TokenClaims
        {
            public const string ROLE = "role";
            public const string UID = "uid";
            public const string EMAIL = "email";
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
    }
}
