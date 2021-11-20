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
    internal class OSMTerrainGenerator : OSMStructureGenerator
    {
        #region FIELDS

        private Terrain _terrain;
        private bool _generateWater;
        private Color _terrainMatColor;
        private Material _defaultMaterial, _grassMaterial, _dirtMaterial;
        private Color _waterMatColor = Color.blue;
        private Material _waterMaterial;

        #endregion

        #region PROPERTIES AND INDEXERS

        private Material DefaultMaterial
        {
            get
            {
                if (_defaultMaterial == null)
                {
                    _defaultMaterial = new Material(DefaultShader);
                    _defaultMaterial.color = _terrainMatColor;
                }

                return _defaultMaterial;
            }
        }

        private Material GrassMaterial
        {
            get { return _grassMaterial != null ? _grassMaterial : DefaultMaterial; }
        }

        private Material DirtMaterial
        {
            get { return _dirtMaterial != null ? _dirtMaterial : DefaultMaterial; }
        }

        private Material WaterMaterial
        {
            get
            {
                if (_waterMaterial == null)
                {
                    _waterMaterial = new Material(WaterShader);
                    _waterMaterial.color = _waterMatColor;
                }

                return _waterMaterial;
            }
        }

        public override int NodeCount
        {
            get { return Map.Terrains.Count; }
        }

        #endregion

        #region CONSTRUCTORS

        public OSMTerrainGenerator(Transform mapRoot, OSMReader map, bool generateWater,
            Material defaultMaterial, Material grassMaterial, Material dirtMaterial, Material waterMaterial) : base(map, mapRoot)
        {
            StructureRoot = new GameObject("OSM TerrainData").transform;
            _generateWater = generateWater;
            _defaultMaterial = defaultMaterial;
            _grassMaterial = grassMaterial;
            _dirtMaterial = dirtMaterial;
            _waterMaterial = waterMaterial;
        }

        #endregion

        #region METHODS

        public override IEnumerable<int> Generate()
        {
            while (!Map.IsReady)
                yield return 0;

            _terrain = MapRoot.GetComponent<Terrain>();
            if (_terrain == null)
                _terrain = MapRoot.gameObject.AddComponent<Terrain>();

            int i = 0;

            // TODO: Generate terrain data

            foreach (OSMTerrain terrain in Map.Terrains)
            {
                Material mat;
                switch (terrain.TerrainType)
                {
                    case OSMTerrain.OSMTerrainType.Unknown:
                        _terrainMatColor = Color.grey;
                        mat = DefaultMaterial;
                        break;
                    case OSMTerrain.OSMTerrainType.Grass:
                        _terrainMatColor = Color.green;
                        mat = GrassMaterial;
                        break;
                    case OSMTerrain.OSMTerrainType.Dirt:
                        _terrainMatColor = new Color(101 / 255f, 67 / 255f, 33 / 255f);
                        mat = DirtMaterial;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GenerateStructure(terrain, mat);

                i++;
                yield return i;
            }
        }

        protected override void OnStructureGeneration(ulong terrainID, Vector3 origin, List<Vector3> vertices, List<Vector3> normals, List<Vector2> uvs, List<int> ids, Terrain terrain)
        {
            Debug.LogWarning("TerrainGen Not implemented");
            return;
            OSMTerrain terrainStructure = Map.Terrains.FirstOrDefault(t => t.ID == terrainID);

            if (terrain == null)
                return;

            Color c = Color.grey;
            OSMNode n1, n2;
            Vector3 vert1, vert2, vert3, vert4;
            int id1, id2, id3, id4;

            for (int i = 1; i < terrainStructure.NodeIDs.Count; i++)
            {
                n1 = Map.Nodes[terrainStructure.NodeIDs[i - 1]];
                n2 = Map.Nodes[terrainStructure.NodeIDs[i]];

                Debug.DrawLine(n1 - Map.BBox.Center, n2 - Map.BBox.Center, c);
            }
        }

        #endregion
    }
}