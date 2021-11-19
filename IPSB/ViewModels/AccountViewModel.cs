using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class AccountVM
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
        public StoreRefModelForAccount Store { get; set; }
        public BuildingRefModelForAccount Building { get; set; }
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
    public class AccountStoreRefModel
    {
        public string Name { get; set; }
    }
    public class AccountSM
    {
        public bool NotManageBuilding { get; set; }
        public bool NotManageStore { get; set; }
        public int BuildingId { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Status { get; set; }
    }
    public class AccountCM
    {
        public string Email { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public int? StoreOwnerBuildingId { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string Role { get; set; }
        
    }
    public class AccountUM
    {
        public string Name { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public bool FirstUpdateProfile { get; set; }
    }
}
