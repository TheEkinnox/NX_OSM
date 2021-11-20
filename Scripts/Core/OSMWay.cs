#region USING

using System.Collections.Generic;
using System.Xml;

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
    public class OSMWay : OSMUtil
    {
        #region ENUMS

        public enum OSMWayType
        {
            Unknown,
            Building,
            Road,
            Terrain,
            Water,
            Forest,
            Boundary
        }

        #endregion

        #region PROPERTIES AND INDEXERS

        public ulong ID { get; private set; }

        public string Name { get; private set; }

        public List<ulong> NodeIDs { get; private set; }

        //public bool IsBoundary { get; private set; }

        public OSMWayType WayType { get; private set; }

        #endregion

        #region CONSTRUCTORS

        public OSMWay(XmlNode xmlNode)
        {
            NodeIDs = new List<ulong>();

            ID = GetAttribute<ulong>("id", xmlNode.Attributes);

            Name = GetTag("name", xmlNode) ?? ID.ToString();

            XmlNodeList nds = xmlNode.SelectNodes("nd");
            foreach (XmlNode node in nds) NodeIDs.Add(GetAttribute<ulong>("ref", node.Attributes));

            //IsBoundary = NodeIDs.Count > 1 && NodeIDs[0] == NodeIDs[NodeIDs.Count - 1];

            WayType = GetWayType(xmlNode);
            if (WayType == OSMWayType.Unknown && NodeIDs.Count > 1 && NodeIDs[0] == NodeIDs[NodeIDs.Count - 1])
                WayType = OSMWayType.Boundary;

            //if (IsWater)
            //    WayType = OSMWayType.Water;
            //else if (IsBuilding)
            //    WayType = OSMWayType.Building;
            //else
            //{
            //    //TODO: Define OSM Way Type
            //    string highwayTagVal = GetTag("highway", xmlNode);
            //    switch (highwayTagVal)
            //    {
            //        case "motorway":
            //            WayType = OSMWayType.Motorway;
            //            break;
            //        case "primary":
            //            WayType = OSMWayType.Primary;
            //            break;
            //        case "secondary":
            //            WayType = OSMWayType.Secondary;
            //            break;
            //        case "tertiary":
            //            WayType = OSMWayType.Tertiary;
            //            break;
            //        case "pedestrian":
            //            WayType = OSMWayType.Pedestrian;
            //            break;
            //        case "footway":
            //        case "cycleway":
            //        case "bridleway":
            //        case "path":
            //            string footwayTagVal = GetTag("footway", xmlNode);
            //            if (footwayTagVal == "sidewalk")
            //                WayType = OSMWayType.Sidewalk;
            //            else if (footwayTagVal == "crossing")
            //                WayType = OSMWayType.Crossing;
            //            else
            //                WayType = OSMWayType.Pathway;
            //            break;
            //        case "railway":
            //            WayType = OSMWayType.Railway;
            //            break;
            //        default:
            //            string landuseTagVal = GetTag("landuse", xmlNode);
            //            string naturalTagVal = GetTag("natural", xmlNode);
            //            string surfaceTagVal = GetTag("surface", xmlNode);
            //            if (landuseTagVal == "forest" 
            //                || naturalTagVal == "wood")
            //                WayType = OSMWayType.Forest;
            //            else if (landuseTagVal == "meadow"
            //            || naturalTagVal == "grassland"
            //            || surfaceTagVal == "ground"
            //            || surfaceTagVal == "grass")
            //                WayType = OSMWayType.Grass;
            //            else if (surfaceTagVal == "dirt"
            //            || surfaceTagVal == "earth")
            //                WayType = OSMWayType.Grass;
            //            else if (surfaceTagVal != null)
            //                WayType = OSMWayType.DefaultGround;
            //            else
            //                WayType = OSMWayType.Unknown;
            //            break;
            //    }
            //}
        }

        #endregion
    }
}