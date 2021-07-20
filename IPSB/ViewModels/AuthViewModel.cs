using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class AuthWebLogin
    {
        public string Email { get; set; }
        public string Password { get; set; }

    }
    
    public class AuthWebChangePassword
    {
        public int AccountId { get; set; }
        public string Password { get; set; }

    }
    
}
