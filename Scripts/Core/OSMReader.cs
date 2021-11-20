#region USING

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;
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

namespace NX_OSM.Core
{
    internal sealed class OSMReader : OSMUtil
    {
        #region CONSTANTS AND STATIC FIELDS

        public const string BoundsNode = "/osm/bounds";
        public const string WayNode = "/osm/way";
        public const string BaseNode = "/osm/node";

        #endregion

        #region PROPERTIES AND INDEXERS

        public Dictionary<ulong, OSMNode> Nodes { get; private set; }
        public List<OSMWay> Ways { get; private set; }
        public OSMBoundingBox BBox { get; private set; }

        public List<OSMBuilding> Buildings { get; private set; }
        public List<OSMRoad> Roads { get; private set; }
        public List<OSMTerrain> Terrains { get; private set; }

        public bool IsReady { get; private set; }

        #endregion

        #region METHODS

        /// <summary>
        /// Read and load the information from provided osm source
        /// </summary>
        /// <param name="osmSource">The url or path to the osm data</param>
        /// <param name="isOnline">Wether the source is online or not</param>
        /// <param name="loadBuildings">Whether buildings should be loaded or not</param>
        /// <param name="loadRoads">Whether roads should be loaded or not</param>
        /// <param name="loadTerrain">Whether terrain data should be loaded or not</param>
        /// <param name="loadWater">Whether water should be loaded or not</param>
        /// <param name="loadForests">Whether forests should be loaded or not</param>
        internal IEnumerable<float> Read(string osmSource, bool isOnline,
            bool loadBuildings, bool loadRoads, bool loadTerrain,
            bool loadWater, bool loadForests)
        {
            IsReady = false;
            int actionsCount = 5;
            float currentAction = 0;

            if (!loadBuildings && !loadRoads && !loadTerrain && !loadWater && !loadForests)
            {
                Debug.LogError("Nothing to load...");
            }
            else
            {
                Nodes = new Dictionary<ulong, OSMNode>();
                Buildings = new List<OSMBuilding>();
                Roads = new List<OSMRoad>();
                Terrains = new List<OSMTerrain>();
                Ways = new List<OSMWay>();

                string fileContent = isOnline ? new WebClient().DownloadString(osmSource) : File.ReadAllText(osmSource);
                yield return ++currentAction / actionsCount;

                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(fileContent);
                yield return ++currentAction / actionsCount;

                LoadBBox(xmlDoc.SelectSingleNode(OSMReader.BoundsNode));
                yield return ++currentAction / actionsCount;
                LoadWays(xmlDoc.SelectNodes(OSMReader.WayNode), loadBuildings, loadRoads, loadTerrain, loadWater, loadForests);
                yield return ++currentAction / actionsCount;
                LoadBaseNodes(xmlDoc.SelectNodes(OSMReader.BaseNode));
                yield return 1;
            }

            IsReady = true;
        }

        internal void DrawDebug(bool loadBuildings, bool loadRoads,
            bool loadTerrain, bool loadWater, bool loadForests, Vector3 center)
        {
            if (!IsReady)
            {
                Debug.LogError("The map data hasn't been loaded yet...");
                return;
            }

            OSMNode n1, n2;
            Color c;
            if (loadTerrain)
            {
                c = Color.grey;
                foreach (OSMTerrain terrain in Terrains)
                    for (int i = 1; i < terrain.NodeIDs.Count; i++)
                    {
                        n1 = Nodes[terrain.NodeIDs[i - 1]];
                        n2 = Nodes[terrain.NodeIDs[i]];

                        Debug.DrawLine(n1 - BBox.Center + center, n2 - BBox.Center + center, c);
                    }
            }

            if (loadBuildings)
                foreach (OSMBuilding building in Buildings)
                {
                    switch (building.BuildingType)
                    {
                        case OSMBuilding.OSMBuildingType.Unknown:
                            c = new Color(101 / 255f, 67 / 255f, 33 / 255f);
                            break;
                        case OSMBuilding.OSMBuildingType.House:
                            c = Color.cyan;
                            break;
                        case OSMBuilding.OSMBuildingType.Flat:
                            c = Color.red;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    for (int i = 1; i < building.NodeIDs.Count; i++)
                    {
                        n1 = Nodes[building.NodeIDs[i - 1]];
                        n2 = Nodes[building.NodeIDs[i]];

                        Debug.DrawLine(n1 - BBox.Center + center, n2 - BBox.Center + center, c);
                    }
                }

            if (loadRoads)
            {
                c = Color.magenta;
                foreach (OSMRoad road in Roads)
                    for (int i = 1; i < road.NodeIDs.Count; i++)
                    {
                        n1 = Nodes[road.NodeIDs[i - 1]];
                        n2 = Nodes[road.NodeIDs[i]];

                        Debug.DrawLine(n1 - BBox.Center + center, n2 - BBox.Center + center, c);
                    }
            }

            foreach (OSMWay way in Ways)
            {
                switch (way.WayType)
                {
                    case OSMWay.OSMWayType.Water:
                        if (!loadWater)
                            return;
                        c = Color.blue;
                        break;
                    case OSMWay.OSMWayType.Forest:
                        if (!loadForests)
                            return;
                        c = Color.green;
                        break;
                    case OSMWay.OSMWayType.Boundary:
                        c = Color.black;
                        break;
                    case OSMWay.OSMWayType.Unknown:
                        //Debug.Log("Unknown : " + way.ID);
                        c = Color.yellow;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                for (int i = 1; i < way.NodeIDs.Count; i++)
                {
                    n1 = Nodes[way.NodeIDs[i - 1]];
                    n2 = Nodes[way.NodeIDs[i]];

                    Debug.DrawLine(n1 - BBox.Center + center, n2 - BBox.Center + center, c);
                }
            }
        }

        /// <summary>
        /// Loads the bounding box from the provided xmlNode
        /// </summary>
        /// <param name="xmlNode">The bounding box node</param>
        private void LoadBBox(XmlNode xmlNode)
        {
            BBox = new OSMBoundingBox(xmlNode);
        }

        /// <summary>
        /// Loads the OSM Way nodes from the provided node list
        /// </summary>
        /// <param name="xmlNodeList">The OSM Ways node list</param>
        /// <param name="loadBuildings"></param>
        /// <param name="loadRoads"></param>
        /// <param name="loadTerrain"></param>
        /// <param name="loadWater"></param>
        /// <param name="loadForests"></param>
        private void LoadWays(XmlNodeList xmlNodeList, bool loadBuildings,
            bool loadRoads, bool loadTerrain,
            bool loadWater, bool loadForests)
        {
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                OSMWay.OSMWayType wayType = GetWayType(xmlNode);
                switch (wayType)
                {
                    case OSMWay.OSMWayType.Unknown:
                        Ways.Add(new OSMWay(xmlNode));
                        break;
                    case OSMWay.OSMWayType.Building:
                        if (loadBuildings)
                            Buildings.Add(new OSMBuilding(xmlNode));
                        break;
                    case OSMWay.OSMWayType.Road:
                        if (loadRoads)
                            Roads.Add(new OSMRoad(xmlNode));
                        break;
                    case OSMWay.OSMWayType.Terrain:
                        if (loadTerrain)
                            Terrains.Add(new OSMTerrain(xmlNode));
                        break;
                    case OSMWay.OSMWayType.Water:
                        if (loadWater)
                            Ways.Add(new OSMWay(xmlNode));
                        break;
                    case OSMWay.OSMWayType.Forest:
                        if (loadForests)
                            Ways.Add(new OSMWay(xmlNode));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Loads the OSM nodes from the provided node list
        /// </summary>
        /// <param name="xmlNodeList">The OSM node list</param>
        private void LoadBaseNodes(XmlNodeList xmlNodeList)
        {
            foreach (XmlNode xmlNode in xmlNodeList)
            {
                OSMNode node = new OSMNode(xmlNode);
                Nodes[node.ID] = node;
            }
        }

        #endregion
    }
}