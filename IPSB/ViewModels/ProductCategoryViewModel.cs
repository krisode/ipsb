using IPSB.Infrastructure.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class ProductCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductVM> Products { get; set; }
    }
        public class ProductCategorySM
    {
        public string Name { get; set; }
    }

    public class ProductCategoryCM
    {
        public string Name { get; set; }
    }

    public class ProductCategoryUM
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
