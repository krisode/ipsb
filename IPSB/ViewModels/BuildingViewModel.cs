using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class BuildingVM
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int AdminId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int NumberOfFloor { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }

        public AccountRefModel Admin { get; set; }
        public AccountRefModel Manager { get; set; }
        public ICollection<FloorPlanRefModel> FloorPlans { get; set; }
        public ICollection<StoreRefModel> Stores { get; set; }
        public ICollection<VisitRouteRefModel> VisitRoutes { get; set; }
    }
    public class BuildingRefModel
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int AdminId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int NumberOfFloor { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
    public class BuildingStoreRefModel
    {
        public string Name { get; set; }
    }

    public class BuildingSM
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int AdminId { get; set; }
        public string Name { get; set; }
        public int NumberOfFloor { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }
    }
    public class BuildingCM
    {
        [Required]
        public int ManagerId { get; set; }
        [Required]
        public int AdminId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public IFormFile ImageUrl { get; set; }
        [Required]
        public int NumberOfFloor { get; set; }
        [Required]
        public string Address { get; set; }
    }
    public class BuildingUM
    {
        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int AdminId { get; set; }
        public string Name { get; set; }
        public IFormFile ImageUrl { get; set; }
        public int NumberOfFloor { get; set; }
        public string Address { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
