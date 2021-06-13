using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class AccountCM
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public IFormFile Image { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
}
