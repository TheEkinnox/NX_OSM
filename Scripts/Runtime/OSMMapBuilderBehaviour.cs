using System.Collections;
using System.Collections.Generic;
using NX_OSM.Generation;
using UnityEngine;

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

namespace NX_OSM.Runtime
{
    public class OSMMapBuilderBehaviour : MonoBehaviour
    {
        private OSMMapBuilder _mapBuilder;
        public bool IsLoading { get; private set; }

        [Header("Source settings")] [SerializeField]
        private string _sourcePath;

        [SerializeField] private bool _isOnine;

        [Header("Generation settings")] [SerializeField]
        private bool _drawDebug;

        [SerializeField] private bool _loadBuildings;
        [SerializeField] private bool _loadRoads;
        [SerializeField] private bool _loadTerrain;
        [SerializeField] private bool _loadWater;
        [SerializeField] private bool _loadForests;

        [Header("Main road materials")] [SerializeField]
        private Material _primaryRoadMaterial;

        [SerializeField] private Material _secondaryRoadMaterial;
        [SerializeField] private Material _tertiaryRoadMaterial;
        [SerializeField] private Material _defaultRoadMaterial;

        [Header("Other road materials")] [SerializeField]
        private Material _pedestrianRoadMaterial;

        [SerializeField] private Material _pathMaterial;
        [SerializeField] private Material _sidewalkMaterial;
        [SerializeField] private Material _crossingMaterial;
        [SerializeField] private Material _railwayMaterial;

        [Header("Terrain materials")] [SerializeField]
        private Material _grassMaterial;

        [SerializeField] private Material _dirtMaterial;
        [SerializeField] private Material _defaultTerrainMaterial;
        [SerializeField] private Material _waterMaterial;

        [Header("Building materials")] [SerializeField]
        private List<Material> _appartmentMaterials;

        [SerializeField] private List<Material> _houseMaterials;
        [SerializeField] private List<Material> _defaultBuildingMaterials;
        [SerializeField] private List<Material> _roofMaterials;
        [SerializeField] private List<Material> _doorMaterials;

        private IEnumerator Start()
        {
            _mapBuilder = new OSMMapBuilder(_sourcePath, _isOnine, gameObject,
                    _primaryRoadMaterial, _secondaryRoadMaterial, _tertiaryRoadMaterial,
                    _pedestrianRoadMaterial, _pathMaterial, _defaultRoadMaterial,
                    _sidewalkMaterial, _crossingMaterial, _railwayMaterial,
                    _grassMaterial, _dirtMaterial, _waterMaterial, _defaultTerrainMaterial,
                    _appartmentMaterials, _houseMaterials, _defaultBuildingMaterials, 
                    _roofMaterials, _doorMaterials);

            IsLoading = true;

            foreach (float f in _mapBuilder.Build(_loadBuildings, _loadRoads, _loadTerrain, _loadWater, _loadForests))
                yield return f;

            IsLoading = false;
        }

        private void FixedUpdate()
        {
            if (_mapBuilder != null && _drawDebug && !IsLoading)
                _mapBuilder.DrawDebug();
        }

        public void Initialize(bool drawDebug, bool loadBuildings, bool loadRoads,
            bool loadTerrain, bool loadWater, bool loadForests, string osmSource, bool isOnline, GameObject rootObject,
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
            // Generation settings 
            _drawDebug = drawDebug;
            _loadBuildings = loadBuildings;
            _loadRoads = loadRoads;
            _loadTerrain = loadTerrain;
            _loadWater = loadWater;
            _loadForests = loadForests;

            // Source settings
            _sourcePath = osmSource;
            _isOnine = isOnline;

            // Materials
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
        }
    }
}