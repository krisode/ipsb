using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Product
    {
        public Product()
        {
            ShoppingItems = new HashSet<ShoppingItem>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int StoreId { get; set; }
        public int ProductCategoryId { get; set; }
        public string ImageUrl { get; set; }
        public string Description { get; set; }
        public double Price { get; set; }
        public int? ProductGroupId { get; set; }
        public string Status { get; set; }

        public virtual ProductCategory ProductCategory { get; set; }
        public virtual ProductGroup ProductGroup { get; set; }
        public virtual Store Store { get; set; }
        public virtual ICollection<ShoppingItem> ShoppingItems { get; set; }
    }
}
