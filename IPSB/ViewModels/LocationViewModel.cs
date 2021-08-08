using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class LocationVM
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public string Status { get; set; }
        public FloorPlanStoreRefModel FloorPlan { get; set; }
        public LocationTypeRefModel LocationType { get; set; }
        public StoreRefModel Store { get; set; }
        public ICollection<EdgeRefModel> EdgeFromLocations { get; set; }
        public ICollection<EdgeRefModel> EdgeToLocations { get; set; }
        public ICollection<LocatorTagRefModel> LocatorTags { get; set; }
        public ICollection<VisitPointRefModel> VisitPoints { get; set; }
    }

    public class LocationRefModel
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public StoreRefModel Store { get; set; }
    }

    public class LocationRefModelForEdge
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public FloorPlanStoreRefModel FloorPlan { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public StoreRefModelForEdge Store { get; set; }
    }

    public class LocationSM
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int BuildingId { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public int NotLocationTypeId { get; set; }
        public int[] LocationTypeIds { get; set; }
        public string LocationTypeName { get; set; }
        public string StoreName { get; set; }
        public string ProductName { get; set; }

    }

    public class LocationCM
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int? StoreId { get; set; }
        public int LocationTypeId { get; set; }

    }

    public class LocationUM
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }

    }

    public class LocationDM
    {
        public List<int> Ids { get; set; }
    }
}
