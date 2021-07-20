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

        public virtual AccountStoreRefModel Account { get; set; }
        public virtual BuildingStoreRefModel Building { get; set; }
        public virtual FloorPlanStoreRefModel FloorPlan { get; set; }
        //public virtual ICollection<CouponRefModel> Coupons { get; set; }
        //public ICollection<ProductCategoryRefModel> ProductCategories { get; set; }
        //public virtual ICollection<FavoriteStoreRefModel> FavoriteStores { get; set; }
        //public virtual ICollection<LocationRefModel> Locations { get; set; }
        //public virtual ICollection<ProductGroupRefModel> ProductGroups { get; set; }
        //public virtual ICollection<ProductRefModel> Products { get; set; }
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
        public ICollection<ProductRefModel> Products { get; set; }
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
        public IFormFileCollection ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string[] ProductCategoryIds { get; set; }
        public string Phone { get; set; }
    }
    public class StoreUM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int AccountId { get; set; }
        public IFormFileCollection ImageUrl { get; set; }
        public int BuildingId { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string[] ProductCategoryIds { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Status { get; set; }
    }
}
