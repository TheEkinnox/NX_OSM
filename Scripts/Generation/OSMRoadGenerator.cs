#region USING

using System;
using System.Collections.Generic;
using System.Linq;
using NX_OSM.Core;
using UnityEngine;

#endregion

/*
    Copyright (c) 2021 Loïck Noa Obiang Ndong (TheEkinnox)

    Permission is hereby granted, free of charge, to any person obtaining a copy
    of this software and associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
    copies of the Software, and to permit persons to whom the Software is
    furnished to do so, subject to the following conditions:

    The above copyright notice and this permission notice shall be included in all
    copies or substantial portions of the Software.

    THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
    IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
    LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
    OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
    SOFTWARE.
 */

namespace NX_OSM.Generation
{
    internal class OSMRoadGenerator : OSMStructureGenerator
    {
        #region FIELDS

        private Color _roadMatColor = Color.black;
        private Material _primaryMat, _secondaryMat, _tertiaryMat, _defaultMat;

        #endregion

        #region PROPERTIES AND INDEXERS

        public override int NodeCount
        {
            get { return Map.Roads.Count; }
        }

        private Material DefaultMat
        {
            get
            {
                if (_defaultMat == null)
                {
                    _defaultMat = new Material(DefaultShader);
                    _defaultMat.color = _roadMatColor;
                }

                return _defaultMat;
            }
        }

        private Material PrimaryMat
        {
            get { return _primaryMat != null ? _primaryMat : DefaultMat; }
        }

        private Material SecondaryMat
        {
            get { return _secondaryMat != null ? _secondaryMat : DefaultMat; }
        }

        private Material TertiaryMat
        {
            get { return _tertiaryMat != null ? _tertiaryMat : DefaultMat; }
        }

        #endregion

        #region CONSTRUCTORS

        public OSMRoadGenerator(Transform mapRoot, OSMReader map, Material defaultMat, Material primaryMat, Material secondaryMat, Material tertiaryMat) : base(map, mapRoot)
        {
            StructureRoot = new GameObject("OSM Roads").transform;
            _defaultMat = defaultMat;
            _primaryMat = primaryMat;
            _secondaryMat = secondaryMat;
            _tertiaryMat = tertiaryMat;
        }

        #endregion

        #region METHODS

        public override IEnumerable<int> Generate()
        {
            while (!Map.IsReady)
                yield return 0;
            int i = 0;
            foreach (OSMRoad road in Map.Roads)
            {
                Material mat = DefaultMat;
                ;
                switch (road.RoadType)
                {
                    case OSMRoad.OSMRoadType.Unknown:
                        mat = DefaultMat;
                        break;
                    case OSMRoad.OSMRoadType.Motorway:
                    case OSMRoad.OSMRoadType.Primary:
                        mat = PrimaryMat;
                        break;
                    case OSMRoad.OSMRoadType.Secondary:
                        mat = SecondaryMat;
                        break;
                    case OSMRoad.OSMRoadType.Tertiary:
                        mat = TertiaryMat;
                        break;
                    case OSMRoad.OSMRoadType.Pathway:
                        break;
                    case OSMRoad.OSMRoadType.Pedestrian:
                        break;
                    case OSMRoad.OSMRoadType.Sidewalk:
                        break;
                    case OSMRoad.OSMRoadType.Crossing:
                        break;
                    case OSMRoad.OSMRoadType.Railway:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GenerateStructure(road, mat);

                i++;
                yield return i;
            }
        }

        protected override void OnStructureGeneration(ulong roadID, Vector3 origin, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> ids, Terrain terrain = null)
        {
            OSMRoad road = Map.Roads.FirstOrDefault(b => b.ID == roadID);

            if (road == null)
                return;

            Vector3 n1, n2, vert1, vert2, vert3, vert4;
            int id1, id2, id3, id4;

            /* TODO: Generate a spline-based road (https://www.youtube.com/watch?v=o9RK6O2kOKo&ab_channel=Unity)
             * or just linked triangles... https://www.youtube.com/watch?v=Q12sb-sOhdI&ab_channel=SebastianLague
             */
            for (int i = 1; i < road.NodeIDs.Count; i++)
            {
                n1 = Map.Nodes[road.NodeIDs[i - 1]] - origin;
                n2 = Map.Nodes[road.NodeIDs[i]] - origin;

                float roadLength = Vector3.Distance(n1, n2);

                Vector3 dir = (n2 - n1).normalized;
                Vector3 left = new Vector3(dir.z, dir.y, -dir.x);
                float roadWith = road.Width * road.LaneCount;

                vert1 = n1 + left * (roadWith / 2) + Vector3.up * GetTerrainHeight(terrain, n1); // start left
                vert2 = n2 + left * (roadWith / 2) + Vector3.up * GetTerrainHeight(terrain, n2); // end left
                vert3 = vert1 - left * roadWith; // start right
                vert4 = vert2 - left * roadWith; // end right

                vertices.Add(vert1);
                vertices.Add(vert2);
                vertices.Add(vert3);
                vertices.Add(vert4);

                for (int j = 0; j < 4; j++)
                    normals.Add(Vector3.up);

                id1 = vertices.Count - 4; // vert1
                id2 = vertices.Count - 3; // vert2
                id3 = vertices.Count - 2; // vert3
                id4 = vertices.Count - 1; // vert4

                // Generate triangles (oof)
                ids.Add(id1);
                ids.Add(id3);
                ids.Add(id2);

                ids.Add(id2);
                ids.Add(id3);
                ids.Add(id4);

                // Generate uvs
                uvs.Add(new Vector2(0, 0)); // Start left corner
                uvs.Add(new Vector2(0, roadLength)); // End left corner
                uvs.Add(new Vector2(1, 0)); // Start right corner
                uvs.Add(new Vector2(1, roadLength)); // End right corner
            }
        }

        #endregion
    }
}