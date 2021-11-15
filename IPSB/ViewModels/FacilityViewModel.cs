using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class FacilityVM
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int FloorPlanId { get; set; }
        public int BuildingId { get; set; }
        public virtual LocationRefModel Location { get; set; }
        public FloorPlanStoreRefModel FloorPlan { get; set; }
    }

    public class FacilityRefModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public int FloorPlanId { get; set; }
        public int BuildingId { get; set; }
    }

    public class FacilitySM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int BuildingId { get; set; }
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
        public int? BuildingId { get; set; }
        [Required]
        public int? FloorPlanId { get; set; }
        [Required]
        public string LocationJson { get; set; }
    }

    public class FacilityUM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int FloorPlanId { get; set; }
        public string LocationJson { get; set; }
    }
}

