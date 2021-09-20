

using System;
using System.ComponentModel.DataAnnotations;
using IPSB.Infrastructure.Contexts;

namespace IPSB.ViewModels
{
    public class ShoppingItemVM
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }
        public int ProductId { get; set; }
        public ProductRefModelForShoppingItem Product { get; set; }
        public string Note { get; set; }
    }

    public class ShoppingItemSM
    {
        public int ShoppingListId { get; set; }
        public string Note { get; set; }
    }
    public class ShoppingItemCM
    {
        [Required]
        public int ShoppingListId { get; set; }
        [Required]
        public int ProductId { get; set; }
        public string Note { get; set; }
    }
    public class ShoppingItemUM
    {
        [Required]
        public int ProductId { get; set; }
        public string Note { get; set; }
    }

}