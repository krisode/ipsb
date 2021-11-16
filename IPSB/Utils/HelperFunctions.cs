using System;

namespace IPSB.Utils
{
    public class HelperFunctions
    {
        public static double DistanceBetweenLatLng(double fromLat, double fromLng, double toLat, double toLng)
        {
            return 12742.02 * Math.Asin(Math.Sqrt(Math.Sin(((Math.PI / 180) * (fromLat - toLat)) / 2) * Math.Sin(((Math.PI / 180) * (fromLat - toLat)) / 2) +
                                    Math.Cos((Math.PI / 180) * toLat) * Math.Cos((Math.PI / 180) * (fromLat)) *
                                    Math.Sin(((Math.PI / 180) * (fromLng - toLng)) / 2) * Math.Sin(((Math.PI / 180) * (fromLng - toLng)) / 2)));
        }

        public static double DistanceBetweenTwoPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
    }
}
