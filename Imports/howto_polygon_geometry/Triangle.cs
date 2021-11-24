using UnityEngine;

namespace howto_polygon_geometry
{
    public class Triangle : Polygon
    {
        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            Points = new Vector3[] { p0, p1, p2 };
        }
    }
}
