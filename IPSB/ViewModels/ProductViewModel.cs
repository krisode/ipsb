using Microsoft.AspNetCore.Http;

namespace IPSB.ViewModels
{
    public class ProductVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StoreId { get; set; }
        public int ProductCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int ProductGroupId { get; set; }
        public string Status { get; set; }

        public ProductCategoryRefModel ProductCategory { get; set; }
        public ProductGroupRefModel ProductGroup { get; set; }
        public StoreRefModel Store { get; set; }
    }
    public class ProductRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StoreId { get; set; }
        public int ProductCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int ProductGroupId { get; set; }
        public string Status { get; set; }
    }
    public class ProductSM
    {
        public string Name { get; set; }
        public int StoreId { get; set; }
        public int ProductGroupId { get; set; }
        public string Description { get; set; }
        public float LowerPrice { get; set; }
        public float UpperPrice { get; set; }
        public int ProductCategoryId { get; set; }
        public string Status { get; set; }
    }
    public class ProductCM
    {
        public string Name { get; set; }
        public int StoreId { get; set; }
        public int ProductGroupId { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int ProductCategoryId { get; set; }
        public string Status { get; set; } = "Active";
    }
    public class ProductUM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int StoreId { get; set; }
        public int ProductGroupId { get; set; }
        public IFormFile ImageUrl { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public int ProductCategoryId { get; set; }
        public string Status { get; set; } = "Active";
    }
}
