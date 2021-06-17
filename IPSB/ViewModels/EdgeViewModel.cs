using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class EdgeVM
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }

        public LocationRefModel FromLocation { get; set; }
        public LocationRefModel ToLocation { get; set; }
    }

    public class EdgeSM
    {
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double LowerDistance { get; set; }
        public double UpperDistance { get; set; }
        public int FloorPlanId { get; set; }
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
    }

    public class EdgeUM
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }
    }
}
