using GUTGuide.Patterns;
using UnityEngine;

namespace GUTGuide.Utilities
{
    /// <summary>
    /// Root transform for all GUT buildings
    /// </summary>
    public class GutBuildingsParent : LocalSingleton<GutBuildingsParent>
    {
        /// <summary>
        /// Find transform of searching GUT building by its id
        /// </summary>
        /// <param name="id">Id of the GUT building</param>
        /// <returns>The transform of searched building or null if it is not found</returns>
        public Transform GetBuildingTransform(string id)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains(id)) return child;
            }

            return null;
        }
    }
}