using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class ProductCategoryVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<ProductRefModel> Products { get; set; }
    }
    public class ProductCategorySM
    {
        public string Name { get; set; }
    }

    public class ProductCategoryRefModel
    {
        public int Id { get; set; }
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
