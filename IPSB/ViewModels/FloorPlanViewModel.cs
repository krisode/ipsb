using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class FloorPlanVM
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }

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
    }
    public class FloorPlanSM
    {
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }
    }
    public class FloorPlanCM
    {
        public IFormFile ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }
    }
    public class FloorPlanUM
    {
        public int Id { get; set; }
        public IFormFile ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string FloorCode { get; set; }
        public int FloorNumber { get; set; }

    }

}
