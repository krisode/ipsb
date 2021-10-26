using IPSB.Infrastructure.Contexts;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class StoreVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string ProductCategoryIds { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public LocationRefModel Location { get; set; }
        public AccountStoreRefModel Account { get; set; }
        public BuildingRefModelForStore Building { get; set; }
        public FloorPlanStoreRefModel FloorPlan { get; set; }

    }
    public class StoreRefModelForAccount
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class StoreRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string ProductCategoryIds { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
        public BuildingRefModel Building { get; set; }
    }

    public class StoreRefModelForProduct
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public int FloorPlanId { get; set; }
        public ICollection<LocationRefModelForStore> Locations { get; set; }
    }
    public class StoreRefModelForEdge
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public string ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string ProductCategoryIds { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }

    public class StoreSM
    {
        public string Name { get; set; }
        public int AccountId { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string[] ProductCategoryIds { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; }
    }
    public class StoreCM
    {
        public string Name { get; set; }
        public int AccountId { get; set; }
        public IFormFile ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string LocationJson { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string Phone { get; set; }
    }
    public class StoreUM
    {
        public string Name { get; set; }
        public int AccountId { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string LocationJson { get; set; }
        public string Phone { get; set; }
    }
}
