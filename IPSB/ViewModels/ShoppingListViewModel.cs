using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class ShoppingListCM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int BuildingId { get; set; }
        [Required]
        public int AccountId { get; set; }
        [Required]
        public DateTime ShoppingDate { get; set; }
    }
    public class ShoppingListUM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public int BuildingId { get; set; }
        [Required]
        public DateTime ShoppingDate { get; set; }
    }
    public class ShoppingListRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime ShoppingDate { get; set; }
        public BuildingRefModelForShoppingList Building { get; set; }
    }
    public class ShoppingListVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int BuildingId { get; set; }
        public int AccountId { get; set; }
        public DateTime ShoppingDate { get; set; }
        public string Status { get; set; }
        public BuildingRefModel Building { get; set; }
        public List<ShoppingItemVM> ShoppingItems { get; set; }
    }

    public class ShoppingListSM
    {
        public string Name { get; set; }
        public int BuildingId { get; set; }
        public int AccountId { get; set; }
        public DateTime? StartShoppingDate { get; set; }
        public DateTime? EndShoppingDate { get; set; }
        public string Status { get; set; }
        public string NotStatus { get; set; }
    }
}