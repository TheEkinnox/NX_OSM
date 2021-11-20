using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NX_OSM.Generation;
using NX_OSM.Runtime;
using UnityEditor;
using UnityEditor.SceneManagement;
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

namespace NX_OSM.Editor
{
    public class ImporterWindow : EditorWindow
    {
        public enum GenerationMode
        {
            Design,
            Runtime
        }

        private const string FromCoordinatesText = "From coordinates (requires internet connection)";
        private const string FromFileText = "From file";
        private const float DefaultMinLon = 4.3514f, DefaultMinLat = 46.6616f, DefaultMaxLon = 4.3768f, DefaultMaxLat = 46.6710f;
        private const int CoordDecimalCount = 7;
        private readonly float _coordPrecision = float.Parse($".{new string('0', CoordDecimalCount - 1)}1", CultureInfo.InvariantCulture);
        private const int CoordBoxWidth = 96;
        private const int CoordBoxHeight = 24;

        private Vector2 _scrollPos;
        private bool _validPath;

        // Source settings
        private bool _fromFile = true;
        private string _switchSourceText = FromCoordinatesText;
        private string _sourcePath;
        private float _minLon = DefaultMinLon, _minLat = DefaultMinLat, _maxLon = DefaultMaxLon, _maxLat = DefaultMaxLat;

        // Generation settings
        private GenerationMode _genMode;
        private GameObject _rootObject;
        private bool _drawDebug;
        private bool _loadBuildings = true;
        private bool _loadRoads = true;
        private bool _loadTerrain = true;
        private bool _loadWater = true;
        private bool _loadForests = true;
        private bool _showGenSettings;

        // Main road materials
        private Material _primaryRoadMaterial;
        private Material _secondaryRoadMaterial;
        private Material _tertiaryRoadMaterial;
        private Material _defaultRoadMaterial;
        private bool _showMainRoadMats;

        // Other road materials
        private Material _pedestrianRoadMaterial;
        private Material _pathMaterial;
        private Material _sidewalkMaterial;
        private Material _crossingMaterial;
        private Material _railwayMaterial;
        private bool _showOtherRoadMats;

        // Terrain materials
        private Material _grassMaterial;
        private Material _dirtMaterial;
        private Material _defaultTerrainMaterial;
        private Material _waterMaterial;
        private bool _showTerrainMats;

        // Building materials
        [SerializeField] private List<Material> _appartmentMaterials;
        [SerializeField] private List<Material> _houseMaterials;
        [SerializeField] private List<Material> _defaultBuildingMaterials;
        [SerializeField] private List<Material> _roofMaterials;
        [SerializeField] private List<Material> _doorMaterials;

        private bool _isDirtyScene;

        [MenuItem("Window/Import OSM")]
        public static void ShowEditorWindow()
        {
            ImporterWindow window = GetWindow<ImporterWindow>();
            window.titleContent = new GUIContent("Import OSM");
            window.minSize = new Vector2(320, 180);
            window.Show();
        }

        private void OnGUI()
        {
            List<string> errors = new List<string>();
            // Make the window scrollable if it overflows
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            if (GUILayout.Button(_switchSourceText))
            {
                _fromFile = !_fromFile;
                _switchSourceText = _fromFile ? FromCoordinatesText : FromFileText;
            }

            // Show the offline selection GUI (min lat, max lat, min long, max long)
            if (_fromFile)
            {
                if (!File.Exists(_sourcePath))
                {
                    _sourcePath = "None (Choose OSM File)";
                    _validPath = false;
                    errors.Add("Invalid file path : No file selected !");
                }

                GUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField(_sourcePath);
                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("...", GUILayout.MaxWidth(48)))
                {
                    string filePath = EditorUtility.OpenFilePanel("Select osm file", Application.dataPath, null);

                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        _validPath = false;
                        errors.Add("Invalid file path : No file selected !");
                        return;
                    }

                    if (!File.Exists(filePath))
                    {
                        _validPath = false;
                        errors.Add("Invalid file path : The selected file doesn't exist !");
                        return;
                    }

                    _sourcePath = filePath;
                    _validPath = true;
                }

                GUILayout.EndHorizontal();
            }
            // Show the online selection GUI (min lat, max lat, min long, max long)
            else
            {
                try
                {
                    GUILayoutOption[] coordBoxLayout = new GUILayoutOption[]
                    {
                        GUILayout.Width(CoordBoxWidth),
                        GUILayout.Height(CoordBoxHeight)
                    };

                    GUIStyle coordBoxStyle = new GUIStyle(EditorStyles.textField);
                    coordBoxStyle.alignment = TextAnchor.MiddleCenter;

                    //GUILayout.BeginArea(new Rect(10, 10, 100, 100));
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    _maxLat = EditorGUILayout.FloatField(_maxLat, coordBoxStyle, coordBoxLayout);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    _minLon = EditorGUILayout.FloatField(_minLon, coordBoxStyle, coordBoxLayout);
                    GUILayout.Space(CoordBoxWidth);
                    _maxLon = EditorGUILayout.FloatField(_maxLon, coordBoxStyle, coordBoxLayout);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.FlexibleSpace();
                    _minLat = EditorGUILayout.FloatField(_minLat, coordBoxStyle, coordBoxLayout);
                    GUILayout.FlexibleSpace();
                    EditorGUILayout.EndHorizontal();
                    //GUILayout.EndArea();

                    if (Math.Abs(_minLon - _maxLon) < _coordPrecision)
                    {
                        _validPath = false;
                        errors.Add($"Invalid coordinates : Min. longitude and Max. longitude should be different (up to {CoordDecimalCount} decimals) !");
                    }

                    if (_minLon > _maxLon)
                    {
                        _validPath = false;
                        errors.Add("Invalid coordinates : Min. longitude should be smaller than Max. longitude !");
                    }

                    if (Math.Abs(_minLat - _maxLat) < _coordPrecision)
                    {
                        _validPath = false;
                        errors.Add($"Invalid coordinates : Min. latitude and Max. latidue should be different (up to {CoordDecimalCount} decimals) !");
                    }

                    if (_minLat > _maxLat)
                    {
                        _validPath = false;
                        errors.Add("Invalid coordinates : Min. latitude should be smaller than Max. latitude !");
                    }
                }
                catch (Exception e)
                {
                    _validPath = false;
                    //EditorUtility.DisplayDialog($"Error: {e.GetType().Name}", e.Message, "OK");
                    errors.Add($"{e.GetType().Name} : {e.Message}");
                }

                if (errors.Count == 0)
                {
                    string minLonText = _minLon.ToString("0." + new string('#', CoordDecimalCount), CultureInfo.InvariantCulture);
                    string minLatText = _minLat.ToString("0." + new string('#', CoordDecimalCount), CultureInfo.InvariantCulture);
                    string maxLonText = _maxLon.ToString("0." + new string('#', CoordDecimalCount), CultureInfo.InvariantCulture);
                    string maxLatText = _maxLat.ToString("0." + new string('#', CoordDecimalCount), CultureInfo.InvariantCulture);
                    _sourcePath = $"https://overpass-api.de/api/map?bbox={minLonText},{minLatText},{maxLonText},{maxLatText}";
                    _validPath = true;
                }
            }

            // NOTE: Generation settings
            _showGenSettings = EditorGUILayout.BeginFoldoutHeaderGroup(_showGenSettings, "Generation settings");
            if (_showGenSettings)
            {
                _genMode = (GenerationMode) EditorGUILayout.EnumPopup("Generation mode", _genMode);
                _rootObject = (GameObject) EditorGUILayout.ObjectField("OSM Map root", _rootObject, typeof(GameObject), true);
                _drawDebug = EditorGUILayout.Toggle("Draw debug", _drawDebug);
                _loadBuildings = EditorGUILayout.Toggle("Generate buildings", _loadBuildings);
                _loadRoads = EditorGUILayout.Toggle("Generate roads", _loadRoads);
                _loadTerrain = EditorGUILayout.Toggle("Generate terrain", _loadTerrain);
                _loadWater = EditorGUILayout.Toggle("Generate water", _loadWater);
                _loadForests = EditorGUILayout.Toggle("Generate forests", _loadForests);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            // NOTE: Main road materials
            _showMainRoadMats = EditorGUILayout.BeginFoldoutHeaderGroup(_showMainRoadMats, "Main road materials");
            if (_showMainRoadMats)
            {
                _primaryRoadMaterial = (Material) EditorGUILayout.ObjectField("Primary road material", _primaryRoadMaterial, typeof(Material), false);
                _secondaryRoadMaterial = (Material) EditorGUILayout.ObjectField("Secondary road material", _secondaryRoadMaterial, typeof(Material), false);
                _tertiaryRoadMaterial = (Material) EditorGUILayout.ObjectField("Tertiary road material", _tertiaryRoadMaterial, typeof(Material), false);
                _defaultRoadMaterial = (Material) EditorGUILayout.ObjectField("DefaultRoad road material", _defaultRoadMaterial, typeof(Material), false);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            // NOTE: Other road materials
            _showOtherRoadMats = EditorGUILayout.BeginFoldoutHeaderGroup(_showOtherRoadMats, "Other road materials");
            if (_showOtherRoadMats)
            {
                _pedestrianRoadMaterial = (Material) EditorGUILayout.ObjectField("Pedestrian road material", _pedestrianRoadMaterial, typeof(Material), false);
                _pathMaterial = (Material) EditorGUILayout.ObjectField("Pathway material", _pathMaterial, typeof(Material), false);
                _sidewalkMaterial = (Material) EditorGUILayout.ObjectField("Sidewalk material", _sidewalkMaterial, typeof(Material), false);
                _crossingMaterial = (Material) EditorGUILayout.ObjectField("Crossing material", _crossingMaterial, typeof(Material), false);
                _railwayMaterial = (Material) EditorGUILayout.ObjectField("Railway material", _railwayMaterial, typeof(Material), false);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            // NOTE: Terrain materials
            _showTerrainMats = EditorGUILayout.BeginFoldoutHeaderGroup(_showTerrainMats, "Terrain materials");
            if (_showTerrainMats)
            {
                _grassMaterial = (Material) EditorGUILayout.ObjectField("Grass material", _grassMaterial, typeof(Material), false);
                _dirtMaterial = (Material) EditorGUILayout.ObjectField("Dirt material", _dirtMaterial, typeof(Material), false);
                _waterMaterial = (Material) EditorGUILayout.ObjectField("Water material", _waterMaterial, typeof(Material), false);
                _defaultTerrainMaterial = (Material) EditorGUILayout.ObjectField("Default terrain material", _defaultTerrainMaterial, typeof(Material), false);
            }

            EditorGUILayout.EndFoldoutHeaderGroup();

            // NOTE: Building Materials
            SerializedObject so = new SerializedObject(this);

            EditorGUILayout.PropertyField(so.FindProperty("_appartmentMaterials"), true);
            EditorGUILayout.PropertyField(so.FindProperty("_houseMaterials"), true);
            EditorGUILayout.PropertyField(so.FindProperty("_defaultBuildingMaterials"), true);
            EditorGUILayout.PropertyField(so.FindProperty("_roofMaterials"), true);
            EditorGUILayout.PropertyField(so.FindProperty("_doorMaterials"), true);

            so.ApplyModifiedProperties();

            if (_isDirtyScene) EditorGUILayout.HelpBox("The current scene has not been saved yet !", MessageType.Warning);

            foreach (string error in errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Warning);
            }

            //TODO: Import map data
            EditorGUI.BeginDisabledGroup(!_validPath || _isDirtyScene || errors.Count > 0);
            switch (_genMode)
            {
                case GenerationMode.Design:
                    if (_rootObject != null && _rootObject.scene.name == null)
                        EditorGUILayout.HelpBox("Invalid root : Prefabs are not supported for design time generation !", MessageType.Warning);
                    else if (_rootObject != null && !_rootObject.scene.IsValid())
                        EditorGUILayout.HelpBox("Invalid root : The selected root object is not in a valid scene !", MessageType.Warning);
                    else if (GUILayout.Button("Import Map"))
                    {
                        OSMMapBuilder builder = new OSMMapBuilder(_sourcePath, !_fromFile, _rootObject,
                            _primaryRoadMaterial, _secondaryRoadMaterial, _tertiaryRoadMaterial,
                            _pedestrianRoadMaterial, _pathMaterial, _defaultRoadMaterial,
                            _sidewalkMaterial, _crossingMaterial, _railwayMaterial,
                            _grassMaterial, _dirtMaterial, _waterMaterial, _defaultTerrainMaterial,
                            _appartmentMaterials, _houseMaterials, _defaultBuildingMaterials,
                            _roofMaterials, _doorMaterials);

                        foreach (float f in builder.Build(_loadBuildings, _loadRoads, _loadTerrain, _loadWater, _loadForests))
                            EditorUtility.DisplayProgressBar("OSM map generation", builder.LoadingText, f);

                        EditorUtility.ClearProgressBar();
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

                        if (_drawDebug)
                            builder.DrawDebug();
                    }

                    break;
                case GenerationMode.Runtime:
                    if (_rootObject == null)
                        EditorGUILayout.HelpBox("Invalid root : No GameObject provided !", MessageType.Warning);
                    else if (GUILayout.Button("Prepare Builder"))
                    {
                        // TODO: Create a monobehaviour for realtime generation
                        OSMMapBuilderBehaviour builderBehaviour = _rootObject.GetComponent<OSMMapBuilderBehaviour>();
                        if (builderBehaviour == null)
                            builderBehaviour = _rootObject.AddComponent<OSMMapBuilderBehaviour>();
                        builderBehaviour.Initialize(_drawDebug, _loadBuildings, _loadRoads, _loadTerrain, _loadWater, _loadForests,
                            _sourcePath, !_fromFile, _rootObject,
                            _primaryRoadMaterial, _secondaryRoadMaterial, _tertiaryRoadMaterial,
                            _pedestrianRoadMaterial, _pathMaterial, _defaultRoadMaterial,
                            _sidewalkMaterial, _crossingMaterial, _railwayMaterial,
                            _grassMaterial, _dirtMaterial, _waterMaterial, _defaultTerrainMaterial,
                            _appartmentMaterials, _houseMaterials,
                            _defaultBuildingMaterials, _roofMaterials, _doorMaterials);
                        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndScrollView();
        }

        private void Update()
        {
            _isDirtyScene = EditorSceneManager.GetActiveScene().isDirty;
        }
    }
}