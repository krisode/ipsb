using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

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

        public virtual AccountRefModel Account { get; set; }
        public virtual BuildingRefModel Building { get; set; }
        public virtual FloorPlanRefModel FloorPlan { get; set; }
        public virtual ICollection<CouponRefModel> Coupons { get; set; }
        //public ICollection<ProductCategoryRefModel> ProductCategories { get; set; }
        public virtual ICollection<FavoriteStoreRefModel> FavoriteStores { get; set; }
        public virtual ICollection<LocationRefModel> Locations { get; set; }
        public virtual ICollection<ProductGroupRefModel> ProductGroups { get; set; }
        public virtual ICollection<ProductRefModel> Products { get; set; }
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
        public string Status { get; set; } = "Active";
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
        public string Status { get; set; } = "Active";
    }
}
