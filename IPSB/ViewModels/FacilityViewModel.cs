using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class FacilityVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int? LocationId { get; set; }
        public string Status { get; set; }
        public virtual LocationRefModel Location { get; set; }
    }

    public class FacilitySM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int? LocationId { get; set; }
        public string LocationType { get; set; }
        public string Status { get; set; }
    }

    public class FacilityCM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int LocationId { get; set; }
    }

    public class FacilityUM
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public int LocationId { get; set; }
    }
}

