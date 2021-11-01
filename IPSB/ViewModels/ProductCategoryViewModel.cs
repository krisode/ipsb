using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class ProductCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
        public ICollection<ProductRefModel> Products { get; set; }
    }
    public class ProductCategorySM
    {
        public string Name { get; set; }

        public string Status { get; set; }
    }

    public class ProductCategoryRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
    }

    public class ProductCategoryCM
    {
        public string Name { get; set; }

        public IFormFile ImageUrl { get; set; }
    }

    public class ProductCategoryUM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IFormFile ImageUrl { get; set; }

    }
}
