using System.Linq;
using UnityEngine;

namespace GUTGuide.DataStructures
{
    /// <summary>
    /// A data structure that enables to specify information about GUT building
    /// </summary>
    [CreateAssetMenu(fileName = "GUTBuildingData", menuName = "GUTGuide/BuildingData", order = 0)]
    public class GutBuildingData : ScriptableObject
    {
        /// <summary>
        /// Place id from Google Maps metadata
        /// </summary>
        public string id;
        /// <summary>
        /// Full name of the building
        /// </summary>
        public string fullName;
        /// <summary>
        /// Shortcut name of the building
        /// </summary>
        public string shortcut;
        /// <summary>
        /// Id number of the building
        /// </summary>
        [Min(1)] public int number;
        /// <summary>
        /// Address of the building
        /// </summary>
        public string address;

        /// <summary>
        /// Check if the building is GUT building
        /// </summary>
        /// <param name="id">Place id from Google Maps metadata</param>
        /// <returns>True if it is, False if it does not</returns>
        public static bool CheckIsGutBuilding(string id)
        {
            return Resources.LoadAll<GutBuildingData>("GUTBuildingsData").Any(buildingData => buildingData.id == id);
        }
    }
}