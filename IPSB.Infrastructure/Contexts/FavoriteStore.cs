using System;
using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class FavoriteStore
    {
        public int Id { get; set; }
        public int StoreId { get; set; }
        public DateTime RecordDate { get; set; }
        public int VisitCount { get; set; }
        public int BuildingId { get; set; }

        public virtual Store Store { get; set; }
    }
}
