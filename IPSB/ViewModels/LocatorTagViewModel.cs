using System;
using System.ComponentModel.DataAnnotations;

namespace IPSB.ViewModels
{
    public class LocatorTagVM
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public double? TxPower { get; set; }
        public string Status { get; set; }
        public DateTime UpdateTime { get; set; }
        public int? LocatorTagGroupId { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public int BuildingId { get; set; }
        public FloorPlanRefModel FloorPlan { get; set; }
        public LocationRefModel Location { get; set; }
        public LocatorTagRefModel LocatorTagGroup { get; set; }
        
    }
    public class LocatorTagRefModel
    {
        public int Id { get; set; }
        public string Uuid { get; set; }
        public double? TxPower { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int? LocatorTagGroupId { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public int BuildingId { get; set; }
        public string Status { get; set; }
    }
    public class LocatorTagSM
    {
        public int[] Id { get; set; }
        public string Uuid { get; set; }
        public double? TxPower { get; set; }
        public int? LocatorTagGroupId { get; set; }
        public string Status { get; set; }
        public int FloorPlanId { get; set; }
        public int BuildingId { get; set; }
        public int LocationId { get; set; }
        public DateTime? LowerUpdateTime { get; set; }
        public DateTime? UpperUpdateTime { get; set; }
        
    }
    public class LocatorTagCM
    {
        [Required]
        public string Uuid { get; set; }
        [Required]
        public int BuildingId { get; set; }
    }
    public class LocatorTagUM
    {
        public double? TxPower { get; set; }
        public int FloorPlanId { get; set; }
        public string LocationJson { get; set; }
        public int? LocatorTagGroupId { get; set; }
        public int? BuildingId { get; set; }
        
    }

    public class LocatorTagTxPowerUM
    {
        [Required]
        public double TxPower { get; set; }

    }
}
