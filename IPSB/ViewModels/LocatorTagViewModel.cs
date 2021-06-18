using System;

namespace IPSB.ViewModels
{
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
}
