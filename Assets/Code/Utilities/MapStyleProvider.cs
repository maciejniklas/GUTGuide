using System;
using System.Linq;
using Google.Maps;
using GUTGuide.DataStructures;
using GUTGuide.Patterns;
using UnityEngine;

namespace GUTGuide.Utilities
{
    /// <summary>
    /// Provider of available map styles
    /// </summary>
    public class MapStyleProvider : PersistentSingleton<MapStyleProvider>
    {
        [Tooltip("Set of available map styles")]
        [SerializeField] private MapStyleData[] mapStyleData;

        /// <summary>
        /// Receive map style
        /// </summary>
        /// <param name="type"><see cref="MapStyleData.Type"/> of map style</param>
        /// <returns>Map style as <see cref="GameObjectOptions"/></returns>
        /// <exception cref="ArgumentNullException">Thrown when dataset does not contain <see cref="MapStyleData"/>
        /// of given <see cref="MapStyleData.Type"/></exception>
        public GameObjectOptions GetMapStyle(MapStyleData.Type type)
        {
            var styleData = mapStyleData.FirstOrDefault(element => element.StyleType == type);

            if (styleData == null)
            {
                throw new ArgumentNullException(nameof(styleData), $"Style of {type} type does not exist!");
            }
            
            var style = styleData.GenerateStyle();
            return style;

        }
    }
}