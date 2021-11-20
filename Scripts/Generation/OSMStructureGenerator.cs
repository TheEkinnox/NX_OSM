#region USING

using System.Collections.Generic;
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
    internal abstract class OSMStructureGenerator
    {
        #region CONSTANTS AND STATIC FIELDS

        private const string DefaultMapRootName = "OSM Map";

        #endregion

        #region FIELDS

        protected readonly Shader DefaultShader = Shader.Find("Specular");
        protected readonly Shader WaterShader = Shader.Find("Specular");

        protected Transform MapRoot;
        protected OSMReader Map;

        #endregion

        #region PROPERTIES AND INDEXERS

        protected Transform StructureRoot { get; set; }
        public abstract int NodeCount { get; }

        #endregion

        #region CONSTRUCTORS

        protected OSMStructureGenerator(OSMReader map, Transform mapRoot)
        {
            //_root = UnityEngine.Object.FindObjectsOfType<Transform>().FirstOrDefault(t => t.gameObject.name.ToLowerInvariant().StartsWith(OSMMapRootName.ToLowerInvariant()));
            MapRoot = mapRoot;
            Map = map;
        }

        #endregion

        #region METHODS

        public abstract IEnumerable<int> Generate();

        protected Vector3 GetCenter(OSMWay osmWay)
        {
            Vector3 center = Vector3.zero;
            if (osmWay.NodeIDs.Count == 0)
                return center;

            foreach (ulong nodeID in osmWay.NodeIDs)
                center += Map.Nodes[nodeID];

            return center / osmWay.NodeIDs.Count;
        }

        protected void GenerateStructure(OSMWay structure, Material mat)
        {
            // Create the structure gameObject
            GameObject obj = new GameObject(structure.Name);
            obj.transform.SetParent(StructureRoot);
            obj.transform.localPosition = GetCenter(structure) - Map.BBox.Center;
            if (MapRoot != null)
            {
                StructureRoot.SetParent(MapRoot);
                StructureRoot.position = MapRoot.position;
            }

            // Add a mesh filter and a mesh renderer to the object
            MeshFilter meshFilter = obj.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();

            meshRenderer.material = mat;

            // Create the mesh data lists
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> ids = new List<int>();


            // Generate the mesh according based depending on the way type
            OnStructureGeneration(structure.ID, GetCenter(structure), vertices, normals, uvs, ids, MapRoot?.GetComponent<Terrain>());

            Mesh mesh = new Mesh();

            mesh.vertices = vertices.ToArray();
            mesh.normals = normals.ToArray();
            mesh.triangles = ids.ToArray();
            mesh.uv = uvs.ToArray();

            mesh.Optimize();

            // apply the loaded data
            meshFilter.mesh = mesh;
        }

        protected float GetTerrainHeight(Terrain terrain, Vector3 pos)
        {
            if (terrain == null || pos == Vector3.zero)
                return 0;

            return terrain.SampleHeight(terrain.transform.TransformPoint(pos));
        }

        protected abstract void OnStructureGeneration(ulong structureID, Vector3 origin,
            List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> ids,
            Terrain terrain);

        #endregion
    }
}