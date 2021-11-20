#region USING

using System;
using System.Collections.Generic;
using System.Linq;
using NX_OSM.Core;
using UnityEngine;
using Random = UnityEngine.Random;

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
    internal class OSMBuildingGenerator : OSMStructureGenerator
    {
        #region FIELDS

        private Color _buildingMatColor, _roofMatColor, _doorMatColor;
        private List<Material> _defaultMats, _houseMats, _flatMats, _roofMats, _doorMats;

        #endregion

        #region PROPERTIES AND INDEXERS

        public override int NodeCount
        {
            get { return Map.Buildings.Count; }
        }

        private Material DefaultMat
        {
            get
            {
                Material mat = _defaultMats != null && _defaultMats.Count > 0 ? _defaultMats[Random.Range(0, _defaultMats.Count)] : null;
                //Material mat = null;

                if (mat == null)
                {
                    mat = new Material(DefaultShader);
                    mat.color = _buildingMatColor;
                }

                return mat;
            }
        }


        private Material HouseMat
        {
            get { return _houseMats != null && _houseMats.Count > 0 ? _houseMats[Random.Range(0, _houseMats.Count)] : DefaultMat; }
        }

        private Material FlatMat
        {
            get { return _flatMats != null && _flatMats.Count > 0 ? _flatMats[Random.Range(0, _flatMats.Count)] : DefaultMat; }
        }

        private Material RoofMat
        {
            get
            {
                Material mat = _roofMats != null && _roofMats.Count > 0 ? _roofMats[Random.Range(0, _roofMats.Count)] : null;

                if (mat == null)
                {
                    mat = new Material(DefaultShader);
                    mat.color = _roofMatColor;
                }

                return mat;
            }
        }

        private Material DoorMat
        {
            get
            {
                Material mat = _doorMats != null && _doorMats.Count > 0 ? _doorMats[Random.Range(0, _doorMats.Count)] : null;

                if (mat == null)
                {
                    mat = new Material(DefaultShader);
                    mat.color = _doorMatColor;
                }

                return mat;
            }
        }

        #endregion

        #region CONSTRUCTORS

        public OSMBuildingGenerator(Transform mapRoot, OSMReader map, List<Material> appartmentMats, List<Material> houseMats, List<Material> defaultMats,
            List<Material> roofMats, List<Material> doorMats) : base(map, mapRoot)
        {
            StructureRoot = new GameObject("OSM Buildings").transform;
            _flatMats = appartmentMats;
            _houseMats = houseMats;
            _defaultMats = defaultMats;
            _roofMats = roofMats;
            _doorMats = doorMats;
        }

        #endregion

        #region METHODS

        public override IEnumerable<int> Generate()
        {
            while (!Map.IsReady)
                yield return 0;

            int i = 0;
            foreach (OSMBuilding building in Map.Buildings)
            {
                Material mat;
                switch (building.BuildingType)
                {
                    case OSMBuilding.OSMBuildingType.Unknown:
                        _buildingMatColor = new Color(101 / 255f, 67 / 255f, 33 / 255f);
                        _roofMatColor = _buildingMatColor;
                        _doorMatColor = new Color(.753f, .753f, .753f);
                        mat = DefaultMat;
                        break;
                    case OSMBuilding.OSMBuildingType.House:
                        _buildingMatColor = new Color(.961f, .961f, .863f);
                        _roofMatColor = new Color(.7176f, .3608f, .3333f);
                        _doorMatColor = new Color(.7922f, .6431f, .4471f);
                        mat = HouseMat;
                        break;
                    case OSMBuilding.OSMBuildingType.Flat:
                        _buildingMatColor = new Color(.796f, .255f, .329f);
                        _roofMatColor = new Color(.5373f, .1765f, .1961f);
                        _doorMatColor = new Color(.6588f, .8f, .8431f, .85f);
                        mat = FlatMat;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GenerateStructure(building, mat);

                i++;
                yield return i;
            }
        }

        protected override void OnStructureGeneration(ulong buildingID, Vector3 origin, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> ids, Terrain terrain)
        {
            OSMBuilding building = Map.Buildings.FirstOrDefault(b => b.ID == buildingID);

            if (building == null)
                return;

            for (int floor = building.MinFloor; floor < building.FloorCount; floor++)
            {
                Vector3 floorCenter = new Vector3(0, building.MinHeight + floor * building.FloorHeight, 0);
                Vector3 ceilingCenter = floorCenter + new Vector3(0, building.FloorHeight - building.MinHeight);

                vertices.Add(floorCenter);
                normals.Add(Vector3.up);
                uvs.Add(new Vector2(.5f, .5f));

                vertices.Add(ceilingCenter);
                normals.Add(Vector3.down);
                uvs.Add(new Vector2(.5f, .5f));

                for (int i = 1; i < building.NodeIDs.Count; i++)
                {
                    Vector3 n1 = Map.Nodes[building.NodeIDs[i - 1]] - origin,
                        n2 = Map.Nodes[building.NodeIDs[i]] - origin;

                    float width = Vector3.Distance(n1, n2);

                    Vector3 vert1 = n1 + new Vector3(0, building.MinHeight + floor * building.FloorHeight + GetTerrainHeight(terrain, n1)), // Bottom start
                        vert2 = n2 + new Vector3(0, building.MinHeight + GetTerrainHeight(terrain, n2)), // Bottom end
                        vert3 = vert1 + new Vector3(0, building.FloorHeight - building.MinHeight), // Top start
                        vert4 = vert2 + new Vector3(0, building.FloorHeight - building.MinHeight); // Top End

                    vertices.Add(vert1);
                    vertices.Add(vert2);
                    vertices.Add(vert3);
                    vertices.Add(vert4);

                    for (int j = 0; j < 4; j++)
                        normals.Add(-Vector3.forward);

                    int id1 = vertices.Count - 4, // vert1
                        id2 = vertices.Count - 3, // vert2
                        id3 = vertices.Count - 2, // vert3
                        id4 = vertices.Count - 1; // vert4

                    // TODO: Generate floor triangles
                    ids.Add(floor - building.MinFloor);
                    ids.Add(id1);
                    ids.Add(id2);

                    ids.Add(id2);
                    ids.Add(id1);
                    ids.Add(floor - building.MinFloor);

                    // Generate wall triangles (oof)
                    ids.Add(id1);
                    ids.Add(id3);
                    ids.Add(id2);

                    ids.Add(id2);
                    ids.Add(id3);
                    ids.Add(id4);

                    // Generate mirrored wall triangles to be sure the mesh is visible from all directions (big oof)
                    ids.Add(id2);
                    ids.Add(id3);
                    ids.Add(id1);

                    ids.Add(id4);
                    ids.Add(id3);
                    ids.Add(id2);

                    // TODO: Generate ceileing triangles
                    ids.Add(floor - building.MinFloor + 1);
                    ids.Add(id3);
                    ids.Add(id4);

                    ids.Add(id4);
                    ids.Add(id3);
                    ids.Add(floor - building.MinFloor + 1);

                    // Generate wall uvs
                    uvs.Add(new Vector2(0, 0)); // Bottom left corner
                    uvs.Add(new Vector2(width, 0)); // Bottom right corner
                    uvs.Add(new Vector2(0, building.FloorHeight - building.MinHeight)); // Top left corner
                    uvs.Add(new Vector2(width, building.FloorHeight - building.MinHeight)); // Top right corner
                }
            }
        }

        #endregion
    }
}