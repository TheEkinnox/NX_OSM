using System;
using System.Globalization;
using System.Xml;

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
    public class OSMRoad : OSMWay
    {
        public enum OSMRoadType
        {
            Unknown,
            Motorway,
            Primary,
            Secondary,
            Tertiary,
            Pathway,
            Pedestrian,
            Sidewalk,
            Crossing,
            Railway
        }

        private const float DefaultWidth = 1.5f;
        private const int DefaultLaneCount = 2;

        public OSMRoadType RoadType { get; private set; }

        public bool IsBridge { get; private set; }

        public bool IsOneWay { get; private set; }

        public float Width { get; private set; }

        public int LaneCount { get; private set; }

        public OSMRoad(XmlNode xmlNode) : base(xmlNode)
        {
            IsBridge = GetTag("bridge", xmlNode) != null;
            IsOneWay = GetTag("oneway", xmlNode) == "yes";

            Width = DefaultWidth;
            string lanesTag = GetTag("lanes", xmlNode);
            if (lanesTag == null || !int.TryParse(lanesTag, out int laneCount))
                LaneCount = DefaultLaneCount;
            else
                LaneCount = laneCount;

            try
            {
                Width = float.Parse(GetTag("width", xmlNode), CultureInfo.InvariantCulture);
                LaneCount = int.Parse(GetTag("lanes", xmlNode), CultureInfo.InvariantCulture);
            }
            //Block ArgumentNullExceptions and FormatExceptions as the default values are already assigned
            catch (ArgumentNullException) {}
            catch (FormatException) {}

            // TODO: Define road type
            string highwayTagVal = GetTag("highway", xmlNode);
            switch (highwayTagVal)
            {
                case "motorway":
                    RoadType = OSMRoadType.Motorway;
                    break;
                case "primary":
                    RoadType = OSMRoadType.Primary;
                    break;
                case "secondary":
                    RoadType = OSMRoadType.Secondary;
                    break;
                case "tertiary":
                    RoadType = OSMRoadType.Tertiary;
                    break;
                case "pedestrian":
                    RoadType = OSMRoadType.Pedestrian;
                    break;
                case "footway":
                case "cycleway":
                case "bridleway":
                case "path":
                    string footwayTagVal = GetTag("footway", xmlNode);
                    if (footwayTagVal == "sidewalk")
                        RoadType = OSMRoadType.Sidewalk;
                    else if (footwayTagVal == "crossing")
                        RoadType = OSMRoadType.Crossing;
                    else
                        RoadType = OSMRoadType.Pathway;
                    break;
                case "railway":
                    RoadType = OSMRoadType.Railway;
                    break;
                default:
                    RoadType = OSMRoadType.Unknown;
                    break;
            }
        }
    }
}