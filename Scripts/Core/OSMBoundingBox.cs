#region USING

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
    /// <summary>
    /// An OSM map bounding box
    /// </summary>
    public class OSMBoundingBox : OSMUtil
    {
        #region PROPERTIES AND INDEXERS

        /// <summary>
        /// Minimum latitude (y-axis)
        /// </summary>
        public float MinLat { get; private set; }

        /// <summary>
        /// Maximum latitude (y-axis)
        /// </summary>
        public float MaxLat { get; private set; }

        /// <summary>
        /// Minimum longitude (x-axis)
        /// </summary>
        public float MinLon { get; private set; }

        /// <summary>
        /// Maximum longitude (x-axis)
        /// </summary>
        public float MaxLon { get; private set; }

        /// <summary>
        /// Center of the bounding box in unity coordinates
        /// </summary>
        public Vector3 Center { get; private set; }

        #endregion

        #region CONSTRUCTORS

        /// <summary>
        /// Creates a new osm bounding box representation
        /// </summary>
        /// <param name="xmlNode">The bounding box node</param>
        public OSMBoundingBox(XmlNode xmlNode)
        {
            MinLat = GetAttribute<float>("minlat", xmlNode.Attributes);
            MaxLat = GetAttribute<float>("maxlat", xmlNode.Attributes);
            MinLon = GetAttribute<float>("minlon", xmlNode.Attributes);
            MaxLon = GetAttribute<float>("maxlon", xmlNode.Attributes);

            float xCenter = (float) (MercatorProjection.lonToX(MinLon) + MercatorProjection.lonToX(MaxLon)) / 2;
            float yCenter = (float) (MercatorProjection.latToY(MinLat) + MercatorProjection.latToY(MaxLat)) / 2;

            Center = new Vector3(xCenter, 0, yCenter);
        }

        #endregion
    }
}