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
    public class OSMMapBuilder
    {
        #region FIELDS

        private string _sourcePath;
        private bool _isOnline;
        private Transform _root;
        private Material _primaryRoadMaterial;
        private Material _secondaryRoadMaterial;
        private Material _tertiaryRoadMaterial;
        private Material _pedestrianRoadMaterial;
        private Material _pathMaterial;
        private Material _defaultRoadMaterial;
        private Material _sidewalkMaterial;
        private Material _crossingMaterial;
        private Material _railwayMaterial;
        private Material _grassMaterial;
        private Material _dirtMaterial;
        private Material _defaultTerrainMaterial;
        private Material _waterMaterial;
        private List<Material> _appartmentMaterials;
        private List<Material> _houseMaterials;
        private List<Material> _defaultBuildingMaterials;
        private List<Material> _roofMaterials;
        private List<Material> _doorMaterials;

        private OSMReader _osmReader;

        #endregion

        #region PROPERTIES AND INDEXERS

        public bool IsReady
        {
            get { return _osmReader.IsReady; }
        }

        public string LoadingText { get; private set; }

        #endregion

        #region CONSTRUCTORS

        public OSMMapBuilder(string osmSource, bool isOnline, GameObject rootObject,
            Material primaryRoadMaterial, Material secondaryRoadMaterial, Material tertiaryRoadMaterial,
            Material pedestrianRoadMaterial, Material pathMaterial, Material defaultRoadMaterial,
            Material sidewalkMaterial, Material crossingMaterial, Material railwayMaterial,
            Material grassMaterial, Material dirtMaterial, Material waterMaterial, Material defaultTerrainMaterial,
            List<Material> appartmentMaterials,
            List<Material> houseMaterials,
            List<Material> defaultBuildingMaterials,
            List<Material> roofMaterials,
            List<Material> doorMaterials)
        {
            _sourcePath = osmSource;
            _isOnline = isOnline;
            _root = rootObject != null ? rootObject.transform : null;
            _primaryRoadMaterial = primaryRoadMaterial;
            _secondaryRoadMaterial = secondaryRoadMaterial;
            _tertiaryRoadMaterial = tertiaryRoadMaterial;
            _pedestrianRoadMaterial = pedestrianRoadMaterial;
            _pathMaterial = pathMaterial;
            _defaultRoadMaterial = defaultRoadMaterial;
            _sidewalkMaterial = sidewalkMaterial;
            _crossingMaterial = crossingMaterial;
            _railwayMaterial = railwayMaterial;
            _grassMaterial = grassMaterial;
            _dirtMaterial = dirtMaterial;
            _waterMaterial = waterMaterial;
            _defaultTerrainMaterial = defaultTerrainMaterial;
            _appartmentMaterials = appartmentMaterials;
            _houseMaterials = houseMaterials;
            _defaultBuildingMaterials = defaultBuildingMaterials;
            _roofMaterials = roofMaterials;
            _doorMaterials = doorMaterials;

            _osmReader = new OSMReader();
        }

        #endregion

        #region METHODS

        public IEnumerable<float> Build(bool loadBuildings, bool loadRoads,
            bool loadTerrain, bool loadWater, bool loadForests)
        {
            LoadingText = "Loading OSM data from source (might take a while)";
            foreach (float f in _osmReader.Read(_sourcePath, _isOnline, loadBuildings, loadRoads, loadTerrain, loadWater, loadForests))
                yield return f;

            if (loadTerrain)
            {
                // TODO: Complete terrain generator
                OSMTerrainGenerator terrainGen = new OSMTerrainGenerator(_root, _osmReader, loadWater, _defaultTerrainMaterial, _grassMaterial, _dirtMaterial, _waterMaterial);
                
                foreach(float f in Generate(terrainGen, "terrain data"))
                    yield return f;
            }

            if (loadBuildings)
            {
                // TODO: Complete building generator
                OSMBuildingGenerator buildingGen = new OSMBuildingGenerator(_root, _osmReader, _appartmentMaterials, _houseMaterials, _defaultBuildingMaterials, _roofMaterials, _doorMaterials);

                foreach (float f in Generate(buildingGen, "buildings"))
                    yield return f;
            }

            if (loadRoads)
            {
                // TODO: Complete road generator
                OSMRoadGenerator roadGen = new OSMRoadGenerator(_root, _osmReader, _defaultRoadMaterial, _primaryRoadMaterial, _secondaryRoadMaterial, _tertiaryRoadMaterial);

                foreach (float f in Generate(roadGen, "roads"))
                    yield return f;
            }
        }

        public void DrawDebug(bool loadBuildings = true, bool loadRoads = true,
            bool loadTerrain = true, bool loadWater = true, bool loadForests = true)
        {
            _osmReader.DrawDebug(loadBuildings, loadRoads, loadTerrain, loadWater, loadForests, _root ? _root.position : Vector3.zero);
        }

        private IEnumerable<float> Generate(OSMStructureGenerator structureGen, string generatedStructType)
        {
            int nodeCount = structureGen.NodeCount;
            float progress;

            foreach (int i in structureGen.Generate())
            {
                progress = (float) i / nodeCount;
                LoadingText = $"Generating {generatedStructType ?? "unknown"} ({i}/{nodeCount} -> {progress * 100:0.##}%)";
                yield return progress;
            }
        }

        #endregion
    }
}