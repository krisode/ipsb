using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class VisitStore
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime RecordTime { get; set; }

        public virtual Store Store { get; set; }
    }
}
