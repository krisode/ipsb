using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace IPSB.ViewModels
{

    public class LocationTypeVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<LocationRefModel> Locations { get; set; }
    }

    public class LocationTypeRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string Status { get; set; }
    }

    public class LocationTypeSM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
    }

    public class LocationTypeCM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public IFormFile ImageUrl { get; set; }
    }

    public class LocationTypeUM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public IFormFile ImageUrl { get; set; }
    }
}
