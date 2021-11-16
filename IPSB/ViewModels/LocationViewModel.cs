using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class LocationVM
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DistanceTo { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public string Status { get; set; }
        public FloorPlanStoreRefModel FloorPlan { get; set; }
        public LocationTypeRefModel LocationType { get; set; }
        public StoreRefModel Store { get; set; }
        public LocatorTagRefModel LocatorTag { get; set; }
        public FacilityRefModel Facility { get; set; }
    }

    public class LocationRefModel
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public string Status { get; set; }
        public LocationTypeRefModel LocationType { get; set; }
    }

    public class LocationRefModelForVisitPoint
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public StoreRefModel Store { get; set; }
        public int LocationTypeId { get; set; }
        public string Status { get; set; }
    }

    public class LocationRefModelForStore
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int LocationTypeId { get; set; }
        public int FloorPlanId { get; set; }
        public string Status { get; set; }
    }
    public class LocationRefModelForEdge
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationTypeId { get; set; }
        public StoreRefModelForEdge Store { get; set; }
        public FloorPlanStoreRefModel FloorPlan { get; set; }
        public int StoreId { get; set; }
        public string Status { get; set; }

    }

    public class LocationSM
    {
        public double? X { get; set; }
        public double? Y { get; set; }
        public string SearchKey { get; set; }
        public int BuildingId { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
        public int[] NotLocationTypeIds { get; set; }
        public int[] LocationTypeIds { get; set; }
        public string LocationTypeName { get; set; }
        public string StoreName { get; set; }
        public string ProductName { get; set; }
        public string Status { get; set; }

    }

    public class LocationCM
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationTypeId { get; set; }

    }

    public class LocationUM
    {
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationTypeId { get; set; }

    }

    public class LocationDM
    {
        public List<int> Ids { get; set; }
    }
}
