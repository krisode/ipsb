using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class ShoppingList
    {
        public ShoppingList()
        {
            ShoppingItems = new HashSet<ShoppingItem>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int BuildingId { get; set; }
        public DateTime ShoppingDate { get; set; }
        public string Status { get; set; }

        public virtual Building Building { get; set; }
        public virtual ICollection<ShoppingItem> ShoppingItems { get; set; }
    }
}
