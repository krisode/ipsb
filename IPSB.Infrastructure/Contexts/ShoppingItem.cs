using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class ShoppingItem
    {
        public int Id { get; set; }
        public int ShoppingListId { get; set; }
        public int ProductId { get; set; }
        public string Note { get; set; }

        public virtual Product Product { get; set; }
        public virtual ShoppingList ShoppingList { get; set; }
    }
}
