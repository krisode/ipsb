using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class BuildingVM
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double? DistanceTo { get; set; }
        public string Status { get; set; }
        public AccountRefModel Manager { get; set; }
    }

    public class BuildingRefModelForAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class BuildingRefModel
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Address { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Status { get; set; }
    }
    public class BuildingRefModelForStore
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? DistanceTo { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BuildingRefModelForCoupon
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double? DistanceTo { get; set; }
    }

    public class BuildingRefModelForShoppingList
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
    }

    public class BuildingSM
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public bool FindCurrentBuilding { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Status { get; set; }
    }
    public class BuildingCM
    {
        [Required]
        public int ManagerId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public IFormFile ImageUrl { get; set; }
        [Required]
        public string AddressJson { get; set; }
    }
    public class BuildingUM
    {
        public int ManagerId { get; set; }
        public string Name { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string AddressJson { get; set; }
    }

    public class AddressJson
    {
        public double Lat { get; set; }
        public double Lng { get; set; }
        public string Address { get; set; }
    }
}
