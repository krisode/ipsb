using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class ProductGroup
    {
        public ProductGroup()
        {
            Products = new HashSet<Product>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; }

        public virtual Store Store { get; set; }
        public virtual ICollection<Product> Products { get; set; }
    }
}
