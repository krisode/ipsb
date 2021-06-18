using Microsoft.AspNetCore.Http;

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
