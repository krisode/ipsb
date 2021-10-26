using System.Collections.Generic;

namespace IPSB.ViewModels
{
    public class EdgeVM
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }

        public LocationRefModelForEdge FromLocation { get; set; }
        public LocationRefModelForEdge ToLocation { get; set; }
    }

    public class EdgeSM
    {
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double LowerDistance { get; set; }
        public double UpperDistance { get; set; }
        public int FloorPlanId { get; set; }
        public int BuildingId { get; set; }
        public string Status { get; set; }
    }


    public class EdgeRefModel
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }
    }

    public class EdgeCM
    {
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }
        public int FloorPlanId { get; set; }
        public int LocationTypeId { get; set; }
    }

    public class EdgeUM
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }
    }

    public class EdgeDM
    {
        public List<int> Ids { get; set; }
    }
}
