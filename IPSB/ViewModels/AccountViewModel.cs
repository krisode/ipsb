using Microsoft.AspNetCore.Http;

namespace IPSB.ViewModels
{
    public class AccountVM
    {

    }
    public class AccountRefModel
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }
    public class AccountSM
    {

    }
    public class AccountCM
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public IFormFile Image { get; set; }
        public string Role { get; set; }
        public string Status { get; set; }
    }
    public class AccountUM
    {

    }
}
