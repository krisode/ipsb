using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IPSB.ViewModels
{
    public class VisitPointRefModel
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public int VisitRouteId { get; set; }
        public DateTime RecordTime { get; set; }
    }
}
