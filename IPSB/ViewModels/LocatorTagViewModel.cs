using System;

namespace IPSB.ViewModels
{
    public class LocatorTagVM
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public string Status { get; set; }
        public DateTime UpdateTime { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public DateTime LastSeen { get; set; }

        public FloorPlanRefModel FloorPlan { get; set; }
        public LocationRefModel Location { get; set; }
    }
    public class LocatorTagRefModel
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public string Status { get; set; }
        public DateTime? UpdateTime { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public DateTime? LastSeen { get; set; }
    }
    public class LocatorTagSM
    {
        public int[] Id { get; set; }
        public string MacAddress { get; set; }
        public string Status { get; set; }
        public DateTime? LowerUpdateTime { get; set; }
        public DateTime? UpperUpdateTime { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public DateTime? LowerLastSeen { get; set; }
        public DateTime? UpperLastSeen { get; set; }
    }
    public class LocatorTagCM
    {
        public string MacAddress { get; set; }
        public string Status { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
    }
    public class LocatorTagUM
    {
        public int Id { get; set; }
        public string MacAddress { get; set; }
        public string Status { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationId { get; set; }
        public DateTime? LastSeen { get; set; }
    }
}
