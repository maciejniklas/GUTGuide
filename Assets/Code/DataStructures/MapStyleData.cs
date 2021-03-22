using System;
using Google.Maps;
using Google.Maps.Feature;
using Google.Maps.Feature.Style.Settings;
using UnityEngine;

namespace GUTGuide.DataStructures
{
    /// <summary>
    /// Encapsulates data of styling options for map generated <see cref="GameObject"/>
    /// </summary>
    [Serializable]
    [CreateAssetMenu(fileName = "MapStyleData", menuName = "GUTGuide/MapStyleData")]
    public class MapStyleData : ScriptableObject
    {
        /// <summary>
        /// Style type for different objects styling
        /// </summary>
        public enum Type { Default, Gut }
 
        /// <summary>
        /// Style type for different objects styling
        /// </summary>
        public Type StyleType => type;

        /// <summary>
        /// Style type for different objects styling
        /// </summary>
        [SerializeField] private Type type;
        /// <summary>
        /// The default style applied to generated <see cref="AreaWater"/> <see cref="GameObject"/>s
        /// </summary>
        [SerializeField] private AreaWaterStyleSettings areaWaterStyleSettings;
        /// <summary>
        /// The default style applied to generated <see cref="ExtrudedStructure"/> <see cref="GameObject"/>s
        /// </summary>
        [SerializeField] private ExtrudedStructureStyleSettings extrudedStructureStyleSettings;
        /// <summary>
        /// The default style applied to generated <see cref="LineWater"/> <see cref="GameObject"/>s
        /// </summary>
        [SerializeField] private LineWaterStyleSettings lineWaterStyleSettings;
        /// <summary>
        /// The default style applied to generated <see cref="ModeledStructure"/> <see cref="GameObject"/>s
        /// </summary>
        [SerializeField] private ModeledStructureStyleSettings modeledStructureStyleSettings;
        /// <summary>
        /// The default style applied to generated <see cref="Region"/> <see cref="GameObject"/>s
        /// </summary>
        [SerializeField] private RegionStyleSettings regionStyleSettings;
        /// <summary>
        /// The default style applied to generated <see cref="Segment"/> <see cref="GameObject"/>s
        /// </summary>
        [SerializeField] private SegmentStyleSettings segmentStyleSettings;
        /// <summary>
        /// Materials to apply to buildings walls
        /// </summary>
        [SerializeField] private Material[] wallsMaterials;
        /// <summary>
        /// Materials to apply to roofs
        /// </summary>
        [SerializeField] private Material[] roofMaterials;
        /// <summary>
        /// The <see cref="Material"/> to apply to the parapet once it is created
        /// </summary>
        [SerializeField] private Material parapetMaterial;

        private void OnValidate()
        {
            if (wallsMaterials.Length != roofMaterials.Length)
            {
                Debug.LogError(
                    "There must be the same number of materials for walls and roofs, as we will try to match " +
                    "given Walls to given Roofs (e.g. if a building is given building wall material 2, then it will " +
                    "also be given building roof material 2)");
            }
        }

        /// <summary>
        /// The <see cref="Material"/> to apply to the parapet once it is created
        /// </summary>
        public Material ParapetMaterial => parapetMaterial;
        /// <summary>
        /// Materials to apply to roofs
        /// </summary>
        public Material[] RoofMaterials => roofMaterials;
        /// <summary>
        /// Materials to apply to buildings walls
        /// </summary>
        public Material[] WallsMaterials => wallsMaterials;

        /// <summary>
        /// Generate <see cref="GameObjectOptions"/> from the style data provided in this object
        /// </summary>
        /// <returns><see cref="GameObjectOptions"/> from the style data provided in this object</returns>
        public GameObjectOptions GenerateStyle()
        {
            var style = new GameObjectOptions();
            
            style.AreaWaterStyle = areaWaterStyleSettings.Apply(style.AreaWaterStyle);
            style.ExtrudedStructureStyle = extrudedStructureStyleSettings.Apply(style.ExtrudedStructureStyle);
            style.LineWaterStyle = lineWaterStyleSettings.Apply(style.LineWaterStyle);
            style.ModeledStructureStyle = modeledStructureStyleSettings.Apply(style.ModeledStructureStyle);
            style.RegionStyle = regionStyleSettings.Apply(style.RegionStyle);
            style.SegmentStyle = segmentStyleSettings.Apply(style.SegmentStyle);

            return style;
        }
    }
}