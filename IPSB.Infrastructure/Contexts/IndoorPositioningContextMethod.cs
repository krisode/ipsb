using System;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace IPSB.Infrastructure.Contexts
{
    public partial class IndoorPositioningContext
    {
        [DbFunction("CalcDistanceBetweenLatLng", "dbo")]
        public static double DistanceBetweenLatLng(double fromLat, double fromLng, double toLat, double toLng)
        {
            throw new NotSupportedException();
        }
    }
}
