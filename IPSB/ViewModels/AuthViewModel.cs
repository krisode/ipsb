using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class BaseAuth
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
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

    public class AuthPhoneLogin
    {
        [Required]
        [MinLength(10)]
        public string Phone { get; set; }
        [Required]
        public string Password { get; set; }
    }


    public class AuthLoginSuccess : BaseAuth
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string ImageUrl { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
        public BuildingRefModelForAccount Building { get; set; }
        public StoreRefModelForAccount Store { get; set; }

    }

    public class AuthWebChangePassword
    {
        [Required]
        public int? AccountId { get; set; }
         [Required]
        public string Password { get; set; }

    }
    public class AuthMobileChangePassword
    {
        public int AccountId { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }

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
