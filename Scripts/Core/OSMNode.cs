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
    public class OSMNode : OSMUtil
    {
        #region PROPERTIES AND INDEXERS

        /// <summary>
        /// The node's ID
        /// </summary>
        public ulong ID { get; private set; }

        /// <summary>
        /// The node's name
        /// </summary>
        /// <remarks>If the node doesn't have a name, it's Address or it's ID is considered as such</remarks>
        public string Name { get; private set; }

        /// <summary>
        /// The node's address
        /// </summary>
        /// <remarks>Can be null as not all nodes have an address</remarks>
        public string Address { get; private set; }

        /// <summary>
        /// The latitude of the osm node
        /// </summary>
        public float Latitude { get; private set; }

        /// <summary>
        /// The longitude of the osm node
        /// </summary>
        public float Longitude { get; private set; }

        /// <summary>
        /// The unity position of the node
        /// </summary>
        private Vector3 UnityPos { get; set; }

        #endregion

        #region CONSTRUCTORS

        public OSMNode(XmlNode xmlNode)
        {
            ID = GetAttribute<ulong>("id", xmlNode.Attributes);
            Address = GetAddress(xmlNode);
            Name = GetTag("name", xmlNode) ?? Address ?? ID.ToString();
            ;
            Latitude = GetAttribute<float>("lat", xmlNode.Attributes);
            Longitude = GetAttribute<float>("lon", xmlNode.Attributes);
            UnityPos = new Vector3((float) MercatorProjection.lonToX(Longitude), 0, (float) MercatorProjection.latToY(Latitude));
        }

        #endregion

        #region METHODS

        public static implicit operator Vector3(OSMNode node)
            => node.UnityPos;

        #endregion
    }
}