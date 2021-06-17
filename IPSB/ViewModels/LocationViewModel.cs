using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class LocationRefModel
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int FloorPlanId { get; set; }
        public int StoreId { get; set; }
        public int LocationTypeId { get; set; }
    }
}
