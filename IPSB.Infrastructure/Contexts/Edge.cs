#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class Edge
    {
        public int Id { get; set; }
        public int FromLocationId { get; set; }
        public int ToLocationId { get; set; }
        public double Distance { get; set; }

        public virtual Location FromLocation { get; set; }
        public virtual Location ToLocation { get; set; }
    }
}
