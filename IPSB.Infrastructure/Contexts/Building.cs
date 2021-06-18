using System.Collections.Generic;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Building
    {
        public Building()
        {
            FloorPlans = new HashSet<FloorPlan>();
            Stores = new HashSet<Store>();
            VisitRoutes = new HashSet<VisitRoute>();
        }

        public int Id { get; set; }
        public int ManagerId { get; set; }
        public int AdminId { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int NumberOfFloor { get; set; }
        public string Address { get; set; }
        public string Status { get; set; }

        public virtual Account Admin { get; set; }
        public virtual Account Manager { get; set; }
        public virtual ICollection<FloorPlan> FloorPlans { get; set; }
        public virtual ICollection<Store> Stores { get; set; }
        public virtual ICollection<VisitRoute> VisitRoutes { get; set; }
    }
}
