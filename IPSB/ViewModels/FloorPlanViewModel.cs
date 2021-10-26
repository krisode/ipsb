using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class FloorPlanVM
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }
        public double? RotationAngle { get; set; }
        public double? MapScale { get; set; }
        public string Status { get; set; }
        public BuildingRefModel Building { get; set; }
        public ICollection<LocatorTagRefModel> LocatorTags { get; set; }
        public ICollection<StoreRefModel> Stores { get; set; }
    }

    public class FloorPlanRefModel
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }
        public double? RotationAngle { get; set; }
        public double? MapScale { get; set; }
        public string Status { get; set; }
    }
    public class FloorPlanStoreRefModel
    {
        public string FloorCode { get; set; }
    }
    public class FloorPlanSM
    {
        public int NotFloorPlanId { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }
        public string Status { get; set; }
    }
    public class FloorPlanCM
    {
        [Required]
        public double MapScale { get; set; }
        [Required]
        public IFormFile ImageUrl { get; set; }
        [Required]
        public int BuildingId { get; set; }
        [Required]
        public string FloorCode { get; set; }
        [Required]
        public int FloorNumber { get; set; }
        [Required]
        public double RotationAngle { get; set; }

    }
    public class FloorPlanUM
    {
        public IFormFile ImageUrl { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }
        public double? MapScale { get; set; }
        public double? RotationAngle { get; set; }
        public string Status { get; set; }

    }

}
