﻿using Microsoft.AspNetCore.Http;
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

        public StoreRefModel Store { get; set; }


        public ICollection<BuildingRefModel> BuildingAdmins { get; set; }
        public ICollection<BuildingRefModel> BuildingManagers { get; set; }
        public ICollection<CouponInUseRefModel> CouponInUses { get; set; }
        
        public ICollection<VisitRouteRefModel> VisitRoutes { get; set; }
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
        public int BuildingManagerId { get; set; }
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
        public IFormFile ImageUrl { get; set; }
        public string Role { get; set; }
    }
    public class AccountUM
    {
        public int Id { get; set; }
        public string Role { get; set; }
        public string Name { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
