using System;
using System.Globalization;
using System.Xml;
using OSMWayType = NX_OSM.Core.OSMWay.OSMWayType;

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
    /// <summary>
    /// Utility class for OSM data nodes loading
    /// </summary>
    public class OSMUtil
    {
        /// <summary>
        /// Converts the value of an attribute from a provided collection
        /// into the target type and returns it
        /// </summary>
        /// <typeparam name="T">The target type</typeparam>
        /// <param name="name">The attribute's name</param>
        /// <param name="attributes"></param>
        /// <returns>The converted value of the attribute</returns>
        protected T GetAttribute<T>(string name, XmlAttributeCollection attributes)
        {
            XmlAttribute nodeAttribute = attributes[name];
            if (nodeAttribute == null)
                return default;

            string attributeVal = nodeAttribute.Value;
            return (T) Convert.ChangeType(attributeVal, typeof(T), CultureInfo.InvariantCulture);
        }

        protected string GetTag(string tag, XmlNode xmlNode)
        {
            xmlNode = xmlNode.SelectSingleNode($"tag[@k='{tag}']");
            if (xmlNode?.Attributes == null)
                return null;

            XmlAttribute tagValAttribute = xmlNode.Attributes["v"];
            if (tagValAttribute == null)
                return null;

            if (string.IsNullOrWhiteSpace(tagValAttribute.Value))
                return null;

            return tagValAttribute.Value.ToLowerInvariant();
        }

        /// <summary>
        /// Generates and returns the address string (at least a house number/name and street) from the provided osm node
        /// </summary>
        /// <param name="xmlNode">The osm node</param>
        /// <returns>The generated address string (at least a house number/name and street) </returns>
        protected string GetAddress(XmlNode xmlNode)
        {
            string address;

            string temp = GetTag("addr:full", xmlNode);
            if (!string.IsNullOrWhiteSpace(temp))
                address = temp;
            else
            {
                temp = GetTag("name", xmlNode);
                if (!string.IsNullOrWhiteSpace(temp))
                    address = temp + ' ';
                else
                    address = "";

                temp = GetTag("addr:housenumber", xmlNode);
                if (string.IsNullOrWhiteSpace(temp))
                {
                    temp = GetTag("addr:housename", xmlNode);
                    if (string.IsNullOrWhiteSpace(temp))
                        return null;
                }

                address += temp;

                temp = GetTag("addr:street", xmlNode);
                if (string.IsNullOrWhiteSpace(temp))
                    return null;
                address += $" {temp}";

                temp = GetTag("addr:flats", xmlNode);
                if (!string.IsNullOrWhiteSpace(temp))
                    address += $" {temp}";

                temp = GetTag("addr:postcode", xmlNode);
                if (!string.IsNullOrWhiteSpace(temp))
                    address += $", {temp}";

                temp = GetTag("addr:city", xmlNode);
                if (!string.IsNullOrWhiteSpace(temp))
                    address += $", {temp}";
            }

            return address;
        }

        protected OSMWay.OSMWayType GetWayType(XmlNode xmlNode)
        {
            //TODO: Define OSM Way Type
            string naturalTagVal = GetTag("natural", xmlNode);
            string landuseTagVal = GetTag("landuse", xmlNode);
            string surfaceTagVal = GetTag("surface", xmlNode);
            string typeTagVal = GetTag("type", xmlNode);
            
            if (naturalTagVal == "water" || naturalTagVal == "bay" || naturalTagVal == "strait"
                || naturalTagVal == "spring" || naturalTagVal == "hot_spring"
                || landuseTagVal == "basin" || landuseTagVal == "reservoir"
                || typeTagVal == "waterway" || GetTag("waterway", xmlNode) != null)
                return OSMWay.OSMWayType.Water;
            
            if (GetTag("building", xmlNode) != null || typeTagVal == "building")
                return OSMWay.OSMWayType.Building;
            
            if (GetTag("highway", xmlNode) != null)
                return OSMWay.OSMWayType.Road;
            
            if (landuseTagVal == "forest"
                || naturalTagVal == "wood"
                || naturalTagVal == "tree_row"
                || naturalTagVal == "tree")
                return OSMWay.OSMWayType.Forest;

            if (surfaceTagVal != null
                || landuseTagVal != null
                || naturalTagVal != null
                || GetTag("boundary", xmlNode) != null) 
                return OSMWay.OSMWayType.Terrain;

            return OSMWay.OSMWayType.Unknown;
        }
    }
}