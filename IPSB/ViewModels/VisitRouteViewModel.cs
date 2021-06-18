using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class VisitRouteRefModel
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public DateTime RecordTime { get; set; }
        public int BuildingId { get; set; }
    }
}
