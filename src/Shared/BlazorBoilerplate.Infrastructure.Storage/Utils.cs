using NetTopologySuite.Geometries;

namespace BlazorBoilerplate.Infrastructure.Storage
{
    public static class Utils
    {
        public static Point CreatePoint(double longitude, double latitude)
        {
            return new Point(longitude, latitude) { SRID = 4326 };
        }
    }
}
