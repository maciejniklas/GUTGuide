using System;
using System.Collections.Generic;
using System.Linq;
using Google.Maps;
using Google.Maps.Feature.Style;
using Google.Maps.Terrain;
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
        [Header("General")]
        
        [Tooltip("Set of available map styles")]
        [SerializeField] private MapStyleData[] mapStyleData;
        [Tooltip("The material to apply to the extrusion once it is created")]
        [SerializeField] private Material buildingBorderMaterial;

        [Header("Terrain settings")]
        
        [Tooltip("Material used to paint the control texture from the feature mask")]
        [SerializeField] private Material terrainControlTexture;
        [Tooltip("The target resolution of the alpha map for each generated Terrain tile in meters per pixel. " +
                 "This is the resolution at which TerrainLayer mask painting will occur")]
        [SerializeField] [Min(0.001f)] private float alphaMapResolutionMetersPerPixel = 1f;
        [Tooltip("The target resolution in meters per pixel of the composite texture used on the terrain when " +
                 "viewed from a distance greater than the basemap distance.")]
        [SerializeField] [Min(0.001f)] private float baseMapResolutionMetersPerPixel = 10f;
        [Tooltip("The maximum distance at which terrain textures will be displayed at full resolution")]
        [SerializeField] [Min(1f)] private float baseMapDistance = 2000f;
        [Tooltip("Terrain layers used to style generated terrain. The first layer is applied by default as the base styling layer")]
        [SerializeField] private TerrainLayer[] terrainLayers;

        /// <summary>
        /// The <see cref="Material"/> to apply to the extrusion once it is created
        /// </summary>
        public Material BuildingBorderMaterial => buildingBorderMaterial;
        /// <summary>
        /// <see cref="Material"/> used to paint the control texture from the feature mask
        /// </summary>
        public Material TerrainControlTexture => terrainControlTexture;

        public Material[] GetBuildingMaterials(int index, MapStyleData.Type styleType)
        {
            var styleData = mapStyleData.FirstOrDefault(element => element.StyleType == styleType);

            if (styleData == null) return new Material[0];
            
            var validIndex = index % styleData.WallsMaterials.Length;
            var materials = new[] {styleData.WallsMaterials[validIndex], styleData.RoofMaterials[validIndex]};

            return materials;
        }

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

            // Terrain style generation
            var terrainStyleBuilder = new TerrainStyle.Builder()
            {
                AlphaMapResolutionMetersPerPixel = alphaMapResolutionMetersPerPixel,
                BaseMapResolutionMetersPerPixel = baseMapResolutionMetersPerPixel,
                BaseMapDistance = baseMapDistance,
                TerrainLayers = new List<TerrainLayer>(terrainLayers)
            };
            style.TerrainStyle = terrainStyleBuilder.Build();

            // Segment style with adjustments to the terrain style
            var segmentStyleBuilder = new SegmentStyle.Builder(style.SegmentStyle)
            {
                GameObjectLayer = style.TerrainStyle.TerrainPaintingLayer
            };
            style.SegmentStyle = segmentStyleBuilder.Build();

            // Area water style with adjustments to the terrain style
            var areaWaterStyleBuilder = new AreaWaterStyle.Builder(style.AreaWaterStyle) 
            {
                GameObjectLayer = style.TerrainStyle.TerrainPaintingLayer
            };
            style.AreaWaterStyle = areaWaterStyleBuilder.Build();
            
            // Region style with adjustments to the terrain style
            var regionStyleBuilder = new RegionStyle.Builder(style.RegionStyle) 
            {
                GameObjectLayer = style.TerrainStyle.TerrainPaintingLayer
            };
            style.RegionStyle = regionStyleBuilder.Build();
            
            // Line water style with adjustments to the terrain style
            var lineWaterStyleBuilder = new LineWaterStyle.Builder(style.LineWaterStyle) 
            {
                GameObjectLayer = style.TerrainStyle.TerrainPaintingLayer
            };
            style.LineWaterStyle = lineWaterStyleBuilder.Build();
            
            return style;
        }
    }
}