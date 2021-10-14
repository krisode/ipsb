using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class AuthFirebaseLogin
    {
        [Required]
        public string IdToken { get; set; }
    }
    public class AuthRefreshToken
    {
        public string RefreshToken { get; set; }
    }
    public class AuthWebLogin
    {
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }
        public string Password { get; set; }

    }

    public class AuthPartnerLoginSuccess{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ImageUrl { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public StoreRefModel Store { get; set; }
    }

    public class AuthLoginSuccess{
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ImageUrl { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

    }

    public class AuthWebChangePassword
    {
        public int AccountId { get; set; }
        public string Password { get; set; }

    }

    public class AuthWebForgotPassword
    {
        public string Email { get; set; }

    }

    public class AuthResponseForgotPassword
    {
        public string Url { get; set; }
        public string BackupUrl { get; set; }
    }
    
}
