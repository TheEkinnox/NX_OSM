#region USING

using System;
using System.Collections.Generic;
using System.Linq;
using howto_polygon_geometry;
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
    FITNESS FOR A PARTICULAR PURPOSE AND NON-INFRINGEMENT. IN NO EVENT SHALL THE
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
        private List<Material> _flatMats, _defaultMats, _houseMats, _roofMats, _doorMats;

        #endregion

        #region PROPERTIES AND INDEXERS

        public override int NodeCount => Map.Buildings.Count;

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
            get { return _flatMats != null && _flatMats.Count > 0 ? _flatMats[Random.Range(0, _flatMats.Length)] : DefaultMat; }
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
            
            Dictionary<Vector2, int> floorCorners = new Dictionary<Vector2, int>();
            Dictionary<Vector2, int> ceilingCorners = new Dictionary<Vector2, int>();

            for (int floor = building.MinFloor; floor < building.FloorCount; floor++)
            {
                floorCorners.Clear();
                ceilingCorners.Clear();

                for (int i = 1; i < building.NodeIDs.Count; i++)
                {
                    Vector3 n1 = Map.Nodes[building.NodeIDs[i - 1]] - origin,
                        n2 = Map.Nodes[building.NodeIDs[i]] - origin;

                    GenerateWall(n1, n2, building, floor, vertices, normals, uvs, ids, terrain, floorCorners, ceilingCorners);
                }

                GenerateFloor(floorCorners, vertices, normals, uvs, ids);

                GenerateCeiling(ceilingCorners, vertices, normals, uvs, ids);
            }

            GenerateRoof(ceilingCorners, vertices, building);
        }

        private void GenerateWall(Vector3 point1, Vector3 point2, OSMBuilding building, int floor, ICollection<Vector3> vertices,
            ICollection<Vector3> normals, ICollection<Vector2> uvs, ICollection<int> ids, Terrain terrain, IDictionary<Vector2, int> floorCorners,
            IDictionary<Vector2, int> ceilingCorners)
        {
            float width = Vector3.Distance(point1, point2);

            // Bottom start
            Vector3 vertex = point1 + new Vector3(0, building.MinHeight + floor * building.FloorHeight + GetTerrainHeight(terrain, point1));
            vertices.Add(vertex);
            int id1 = vertices.Count - 1;
            floorCorners[new Vector2(vertex.x, vertex.z)] = id1;

            // Top start
            vertex += new Vector3(0, building.FloorHeight - building.MinHeight);
            vertices.Add(vertex);
            int id2 = vertices.Count - 1;
            ceilingCorners[new Vector2(vertex.x, vertex.z)] = id2;

            // Bottom end
            vertex = point2 + new Vector3(0, building.MinHeight + floor * building.FloorHeight + GetTerrainHeight(terrain, point2));
            vertices.Add(vertex);
            int id3 = vertices.Count - 1;

            // Top end
            vertex += new Vector3(0, building.FloorHeight - building.MinHeight);
            vertices.Add(vertex);
            int id4 = vertices.Count - 1;

            // Generate wall triangles
            ids.Add(id1);
            ids.Add(id2);
            ids.Add(id3);

            ids.Add(id3);
            ids.Add(id2);
            ids.Add(id4);

            // Generate normals
            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);
            normals.Add(Vector3.forward);

            // Generate uvs
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(0, building.FloorHeight - building.MinHeight));
            uvs.Add(new Vector2(width, 0));
            uvs.Add(new Vector2(width, building.FloorHeight - building.MinHeight));

            // Generate mirrored wall triangles to be sure the mesh is visible from all directions
            ids.Add(id4);
            ids.Add(id2);
            ids.Add(id3);

            ids.Add(id3);
            ids.Add(id2);
            ids.Add(id1);
        }

        private void GenerateFloor(Dictionary<Vector2, int> corners, IList<Vector3> vertices, ICollection<Vector3> normals, ICollection<Vector2> uvs, ICollection<int> ids)
        {
            List<Vector3> cornersVerts = new List<Vector3>();
            foreach (KeyValuePair<Vector2, int> corner in corners) 
                cornersVerts.Add(corner.Key);

            Polygon poly = new Polygon(cornersVerts);
            List<Triangle> tris = poly.Triangulate();

            foreach (Triangle triangle in tris)
            {
                Vector3 point1 = vertices[corners[triangle.Points[0]]],
                    point2 = vertices[corners[triangle.Points[1]]],
                    point3 = vertices[corners[triangle.Points[2]]];
                GenerateTriangle(point1, point2, point3, vertices, normals, uvs, ids);
            }
        }

        private void GenerateCeiling(Dictionary<Vector2, int> corners, IList<Vector3> vertices, ICollection<Vector3> normals, ICollection<Vector2> uvs, ICollection<int> ids)
        {
            List<Vector3> cornersVerts = new List<Vector3>();
            foreach (KeyValuePair<Vector2, int> corner in corners) 
                cornersVerts.Add(corner.Key);

            Polygon poly = new Polygon(cornersVerts);
            List<Triangle> tris = poly.Triangulate();

            foreach (Triangle triangle in tris)
            {
                Vector3 point1 = vertices[corners[triangle.Points[0]]],
                    point2 = vertices[corners[triangle.Points[1]]],
                    point3 = vertices[corners[triangle.Points[2]]];
                GenerateTriangle(point3, point2, point1, vertices, normals, uvs, ids);
            }
        }

        private void GenerateRoofBase(Dictionary<Vector2, int> corners, ICollection<Vector3> vertices, IList<Vector3> buildingVertices, ICollection<Vector3> normals, ICollection<Vector2> uvs, ICollection<int> ids)
        {
            List<Vector3> cornersVerts = new List<Vector3>();
            foreach (KeyValuePair<Vector2, int> corner in corners)
                cornersVerts.Add(corner.Key);

            Polygon poly = new Polygon(cornersVerts);
            List<Triangle> tris = poly.Triangulate();

            foreach (Triangle triangle in tris)
            {
                Vector3 point1 = buildingVertices[corners[triangle.Points[2]]],
                    point2 = buildingVertices[corners[triangle.Points[1]]],
                    point3 = buildingVertices[corners[triangle.Points[0]]];

                point1.y = point2.y = point3.y = 0;

                GenerateTriangle(point1, point2, point3, vertices, normals, uvs, ids);
            }
        }

        private void GenerateRoof(Dictionary<Vector2, int> corners, IList<Vector3> buildingVertices, OSMBuilding building)
        {
            GameObject roofObject = new GameObject("Roof");
            roofObject.transform.SetParent(StructureObject.transform);
            Vector3 roofPos = new Vector3(0, building.MinHeight + building.FloorCount * building.FloorHeight, 0);
            float roofHeight = building.RoofFloorCount * building.FloorHeight;
            roofObject.transform.localPosition = roofPos;
            
            // Add a mesh filter and a mesh renderer to the object
            MeshFilter meshFilter = roofObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = roofObject.AddComponent<MeshRenderer>();
            MeshCollider meshCollider = roofObject.AddComponent<MeshCollider>();

            meshRenderer.material = RoofMat;

            // Create the mesh data lists
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> ids = new List<int>();

            GenerateRoofBase(corners, vertices, buildingVertices, normals, uvs, ids);

            List<Vector3> cornersVerts = new List<Vector3>();

            foreach (int cornerIndex in corners.Values)
                cornersVerts.Add(buildingVertices[cornerIndex] - new Vector3(0, building.Height));

            switch (building.RoofShape)
            {
                case OSMBuilding.OSMRoofShape.Flat:
                    break;
                case OSMBuilding.OSMRoofShape.Skillion:
                    break;
                case OSMBuilding.OSMRoofShape.Gabled:
                    break;
                case OSMBuilding.OSMRoofShape.HalfHipped:
                    break;
                case OSMBuilding.OSMRoofShape.Hipped:
                    break;
                case OSMBuilding.OSMRoofShape.Pyramidal:
                    GeneratePyramidalRoof(roofHeight, cornersVerts, vertices, normals, uvs, ids);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = ids.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.Optimize();

            // apply the loaded data
            meshFilter.mesh = mesh;
            meshCollider.sharedMesh = mesh;
        }

        private void GeneratePyramidalRoof(float roofHeight, IList<Vector3> corners, ICollection<Vector3> vertices, ICollection<Vector3> normals, ICollection<Vector2> uvs, ICollection<int> ids)
        {
            Debug.Log("Pyramidal roof");
            Vector3 pyramidTip = new Vector3(0, roofHeight, 0);

            for (int i = 0; i < corners.Count; i++)
            {
                int nextIndex = (i + 1) % corners.Count;

                GenerateTriangle(corners[i], pyramidTip, corners[nextIndex], vertices, normals, uvs, ids);
            }
        }

        #endregion
    }
}