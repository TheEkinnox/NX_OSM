using System;

namespace NX_OSM.Core
{
    /// <summary>
    /// C# Implementation by Florian Müller, based on the C code published above, 14:50, 20.6.2008; updated to static functions by David Schmitt, 23.4.2010
    /// </summary>
    public static class MercatorProjection
    {
        private static readonly double R_MAJOR = 6378137.0;
        private static readonly double R_MINOR = 6356752.3142;
        private static readonly double RATIO = MercatorProjection.R_MINOR / MercatorProjection.R_MAJOR;
        private static readonly double ECCENT = Math.Sqrt(1.0 - (MercatorProjection.RATIO * MercatorProjection.RATIO));
        private static readonly double COM = 0.5 * MercatorProjection.ECCENT;

        private static readonly double DEG2RAD = Math.PI / 180.0;
        private static readonly double RAD2Deg = 180.0 / Math.PI;
        private static readonly double PI_2 = Math.PI / 2.0;

        public static double[] toPixel(double lon, double lat)
        {
            return new double[] { MercatorProjection.lonToX(lon), MercatorProjection.latToY(lat) };
        }

        public static double[] toGeoCoord(double x, double y)
        {
            return new double[] { MercatorProjection.xToLon(x), MercatorProjection.yToLat(y) };
        }

        public static double lonToX(double lon)
        {
            return MercatorProjection.R_MAJOR * MercatorProjection.DegToRad(lon);
        }

        public static double latToY(double lat)
        {
            lat = Math.Min(89.5, Math.Max(lat, -89.5));
            double phi = MercatorProjection.DegToRad(lat);
            double sinphi = Math.Sin(phi);
            double con = MercatorProjection.ECCENT * sinphi;
            con = Math.Pow(((1.0 - con) / (1.0 + con)), MercatorProjection.COM);
            double ts = Math.Tan(0.5 * ((Math.PI * 0.5) - phi)) / con;
            return 0 - MercatorProjection.R_MAJOR * Math.Log(ts);
        }

        public static double xToLon(double x)
        {
            return MercatorProjection.RadToDeg(x) / MercatorProjection.R_MAJOR;
        }

        public static double yToLat(double y)
        {
            double ts = Math.Exp(-y / MercatorProjection.R_MAJOR);
            double phi = MercatorProjection.PI_2 - 2 * Math.Atan(ts);
            double dphi = 1.0;
            int i = 0;
            while ((Math.Abs(dphi) > 0.000000001) && (i < 15))
            {
                double con = MercatorProjection.ECCENT * Math.Sin(phi);
                dphi = MercatorProjection.PI_2 - 2 * Math.Atan(ts * Math.Pow((1.0 - con) / (1.0 + con), MercatorProjection.COM)) - phi;
                phi += dphi;
                i++;
            }
            return MercatorProjection.RadToDeg(phi);
        }

        private static double RadToDeg(double rad)
        {
            return rad * MercatorProjection.RAD2Deg;
        }

        private static double DegToRad(double deg)
        {
            return deg * MercatorProjection.DEG2RAD;
        }
    }
}