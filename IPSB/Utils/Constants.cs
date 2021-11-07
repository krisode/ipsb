using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using System.Text;
using Microsoft.AspNetCore.Http;
using System;

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

        public static class TokenClaims
        {
            public const string ROLE = "role";
            public const string UID = "uid";
            public const string PICTURE = "picture";
            public const string EMAIL = "email";
            public const string NAME = "name";
            public const string PHONE_NUMBER = "phone_number";
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

            public const string READ = "Read";

            public const string UNREAD = "Unread";
        }

        public static class CouponType
        {
            public const int FIXED = 2;

            public const int PERCENTAGE = 3;
        }

        public static class Route {
            public const string FEEDBACK = "/feedback";
            public const string SHOPPING_LIST_DETAIL = "/shoppingListDetail";
            public const string COUPON_DETAIL = "/couponDetail";
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

        public static class JwtBearerTokenConfig
        {
            public static TokenValidationParameters GetTokenValidationParameters(IConfiguration _configuration)
            {
                return new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_configuration[Constants.Config.KEY])),
                    ValidIssuer = _configuration[Constants.Config.ISSUER],
                    ValidAudience = _configuration[Constants.Config.AUDIENCE],

                };
            }
        }

        public static class CookieConfig
        {
            public const string REFRESH_TOKEN = "X-Refresh-Token";
            public static CookieOptions AUTH_COOKIE_OPTIONS = new CookieOptions()
            {
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(TokenParams.DAY_TO_EXPIRES),
                IsEssential = true,
                HttpOnly = true,
                SameSite = SameSiteMode.None,
                Secure = true,
            };
        }

        public static class DefaultValue
        {
            public const int INTEGER = default;
            public const string STRING = default;
            public const float FLOAT = default;
            public const bool BOOLEAN = default;
            public const string PASSWORD = "password123";
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

        public static class CacheConfig
        {
            public const string CACHE_STATUS = "CacheStatus";
        }

        public static class SendEmail
        {
            public const string API_KEY = "d93711a8a4fdcf1f2b24f2df0520bfe2-443ec20e-f4f921fc";

            public const string DOMAIN = "sandbox063d4a6203534601a25434de0bce380b.mailgun.org";

            public const string FROM = "IPSB Team <noreply@notifications.getipsb.com";

        }

        public static class ResponseMessage
        {
            public const string SUCCESS = "Success";

            public const string FAIL = "Fail";

            public const string NOT_FOUND = "Object does not exist."; // 404

            public const string DUPLICATED = "Object already exists."; // 409

            public const string DELETED = "Object has already deleted."; // 400

            public const string UNAUTHORIZE = "Authorization failed."; // 401

            public const string UNAUTHORIZE_READ = "Not authorize to get."; // 403

            public const string UNAUTHORIZE_CREATE = "Not authorize to create."; // 403

            public const string UNAUTHORIZE_UPDATE = "Not authorize to update."; // 403

            public const string UNAUTHORIZE_DELETE = "Not authorize to delete."; // 403

            
            public const string INVALID_PARAMETER = "Invalid parameter: Object"; // 400

            public const string NOT_MODIFIED = "Not modified";

            public const string CAN_NOT_READ = "Can not get due to error."; // 500 or 400

            public const string CAN_NOT_CREATE = "Can not create due to error."; // 500 or 400

            public const string CAN_NOT_UPDATE = "Can not save due to error."; // 500 or 400

            public const string CAN_NOT_DELETE = "Can not delete due to error."; // 500 or 400

            public const string REFRESH_TOKEN = "Refresh Token appeared in both cookie and request body!.";

            public const string REQUIRE_TOKEN = "Require Token in cookie or in request body!.";

        }

        public static class ResponseType
        {
            public const string SUCCESS = "Success";

            public const string FAIL = "Fail";
            
            public const string NOT_FOUND = "NotFoundException";

            public const string INVALID_REQUEST = "One or more validation errors occurred.";

            public const string NOT_MODIFIED = "Not modified";

            public const string UNAUTHORIZE = "OAuthException";

            public const string CAN_NOT_READ = "ExceptionOnFailedGet";

            public const string CAN_NOT_CREATE = "ExceptionOnFailedCreate";

            public const string CAN_NOT_UPDATE = "ExceptionOnFailedSave";

            public const string CAN_NOT_DELETE = "ExceptionOnFailedDelete";
        }

    }
}
