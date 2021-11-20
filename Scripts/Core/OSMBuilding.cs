#region USING

using System;
using System.Globalization;
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
    public class OSMBuilding : OSMWay
    {
        #region ENUMS

        //https://wiki.openstreetmap.org/wiki/Key:building
        public enum OSMBuildingType
        {
            Unknown,
            House,
            Flat
        }

        // https://wiki.openstreetmap.org/wiki/Simple_3D_Buildings
        public enum OSMRoofShape
        {
            Flat,
            Skillion,
            Gabled,
            HalfHipped,
            Hipped,
            Pyramidal
        }

        #endregion

        #region CONSTANTS AND STATIC FIELDS

        private const float DefaultHeight = 3;
        private const float DefaultMinHeight = 0;
        private const int DefaultFloorCount = 1;
        private const int DefaultUndergroundFloorCount = 0;
        private const int DefaultMinFloor = 0;
        private const OSMRoofShape DefaultRoofShape = OSMRoofShape.Flat;

        #endregion

        #region PROPERTIES AND INDEXERS

        private int DefaultRoofFloorCount
        {
            get
            {
                switch (BuildingType)
                {
                    case OSMBuildingType.Unknown:
                        return 0;
                    case OSMBuildingType.House:
                        return 1;
                    case OSMBuildingType.Flat:
                        return 0;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public OSMBuildingType BuildingType { get; private set; }

        public OSMRoofShape RoofShape { get; private set; }

        public float Height { get; private set; }

        public float MinHeight { get; private set; }

        public int FloorCount { get; private set; }

        public float FloorHeight { get; private set; }

        public int UndergroundFloorCount { get; private set; }

        public int RoofFloorCount { get; private set; }

        public int MinFloor { get; private set; }

        #endregion

        #region CONSTRUCTORS

        public OSMBuilding(XmlNode xmlNode) : base(xmlNode)
        {
            string buildingTagVal = GetTag("building", xmlNode);
            switch (buildingTagVal)
            {
                case "apartments":
                case "residential":
                case "dormitory":
                    BuildingType = OSMBuildingType.Flat;
                    break;
                case "house":
                    BuildingType = OSMBuildingType.House;
                    break;
                default:
                    BuildingType = OSMBuildingType.Unknown;
                    break;
            }

            string roofShapeTagVal = GetTag("roof:shape", xmlNode);
            switch (roofShapeTagVal)
            {
                case "flat":
                    RoofShape = OSMRoofShape.Flat;
                    break;
                case "skillion":
                    RoofShape = OSMRoofShape.Skillion;
                    break;
                case "gabled":
                    RoofShape = OSMRoofShape.Gabled;
                    break;
                case "half-hipped":
                    RoofShape = OSMRoofShape.HalfHipped;
                    break;
                case "hipped":
                    RoofShape = OSMRoofShape.Hipped;
                    break;
                case "pyramidal":
                    RoofShape = OSMRoofShape.Pyramidal;
                    break;
                default:
                    RoofShape = OSMBuilding.DefaultRoofShape;
                    break;
            }

            if (!float.TryParse(GetTag("height", xmlNode), NumberStyles.Float, CultureInfo.InvariantCulture, out float height))
                height = OSMBuilding.DefaultHeight;

            if (!float.TryParse(GetTag("min_height", xmlNode), NumberStyles.Float, CultureInfo.InvariantCulture, out float minHeight))
                minHeight = OSMBuilding.DefaultMinHeight;

            if (!int.TryParse(GetTag("building:levels", xmlNode), NumberStyles.Float, CultureInfo.InvariantCulture, out int floorCount))
                floorCount = OSMBuilding.DefaultFloorCount;

            if (!int.TryParse(GetTag("building:levels:underground", xmlNode), NumberStyles.Float, CultureInfo.InvariantCulture, out int undergroundFloorCount))
                undergroundFloorCount = OSMBuilding.DefaultUndergroundFloorCount;

            if (!int.TryParse(GetTag("roof:levels", xmlNode), NumberStyles.Float, CultureInfo.InvariantCulture, out int roofFloorCount))
                roofFloorCount = DefaultRoofFloorCount;

            if (!int.TryParse(GetTag("building:min_level", xmlNode), NumberStyles.Float, CultureInfo.InvariantCulture, out int minFloor))
                minFloor = OSMBuilding.DefaultMinFloor;

            Height = height;
            MinHeight = minHeight;
            FloorCount = floorCount;
            UndergroundFloorCount = undergroundFloorCount;
            RoofFloorCount = roofFloorCount;
            MinFloor = minFloor;
            FloorHeight = Height / FloorCount;
        }

        #endregion
    }
}