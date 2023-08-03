using NetTopologySuite;
using NetTopologySuite.Geometries;

namespace BlazorBoilerplate.Shared.Helpers
{
    public static class TopologyUtils
    {
        private static readonly GeometryFactory gf;
        static TopologyUtils()
        {
            NtsGeometryServices.Instance = new NtsGeometryServices(
                NetTopologySuite.Geometries.Implementation.CoordinateArraySequenceFactory.Instance,
                new PrecisionModel(1000d), 4326);

            gf = NtsGeometryServices.Instance.CreateGeometryFactory();
        }

        public static Point CreatePoint(double longitude, double latitude)
        {
            return gf.CreatePoint(new Coordinate(longitude, latitude));
        }
    }
}
