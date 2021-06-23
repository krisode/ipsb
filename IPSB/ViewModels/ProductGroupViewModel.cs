using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class ProductGroupVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; }

        public StoreRefModel Store { get; set; }
        public ICollection<ProductRefModel> Products { get; set; }
    }
    public class ProductGroupRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; }
    }
    public class ProductGroupSM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; }
    }
    public class ProductGroupCM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; } = "Active";
    }
    public class ProductGroupUM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile Image { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; } = "Active";
    }
}
