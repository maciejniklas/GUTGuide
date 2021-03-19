using System.Collections;
using Google.Maps;
using Google.Maps.Event;
using GUTGuide.DataStructures;
using GUTGuide.Utilities;
using UnityEngine;

namespace GUTGuide.Core
{
    /// <summary>
    /// The central component for map generated events handling
    /// </summary>
    public class MapEventsHandler : MonoBehaviour
    {
        /// <summary>
        /// Default map objects styling
        /// </summary>
        private GameObjectOptions _mapDefaultStyle;
        /// <summary>
        /// Reference to the component responsible for creating road labels
        /// </summary>
        private RoadLabeller _roadLabeller;

        private void Awake()
        {
            _roadLabeller = FindObjectOfType<RoadLabeller>();
        }

        private void Start()
        {
            // Obtain default map objects styling
            _mapDefaultStyle = MapStyleProvider.Instance.GetMapStyle(MapStyleData.Type.Default);
        }

        /// <summary>
        /// Handle <see cref="TerrainEvents.AlphaMapsNeedPaint"/> event by specifying setting the painting coroutine.
        /// </summary>
        public void HandleAlphaMapsNeedPaint(AlphaMapsNeedPaintArgs arguments)
        {
            arguments.PaintingCoroutine = PaintControlTexture(arguments);
        }

        /// <summary>
        /// Handle <see cref="AreaWaterEvents.DidCreate"/> event.
        /// </summary>
        public void HandleDidCreateAreaWater(DidCreateAreaWaterArgs arguments)
        {
            // Obtain parameters necessary for the position set
            var gameObjectPosition = arguments.GameObject.transform.position;

            // Move up the road to avoid z-clipping with regions
            arguments.GameObject.transform.position = gameObjectPosition + Vector3.up;
        }

        /// <summary>
        /// Handle <see cref="SegmentEvents.DidCreate"/> event to edit created segments game objects
        /// </summary>
        public void OnDidCreateExtrudedCallback(DidCreateExtrudedStructureArgs arguments)
        {
            var buildingRenderer = arguments.GameObject.GetComponent<Renderer>();
            var isGutBuilding = GutBuildingData.CheckIsGutBuilding(arguments.MapFeature.Metadata.PlaceId);
            var styleType = isGutBuilding ? MapStyleData.Type.Gut : MapStyleData.Type.Default;
            var materials =
                MapStyleProvider.Instance.GetBuildingMaterials((int) (Random.value * int.MaxValue), styleType);

            buildingRenderer.sharedMaterials = materials;
        }

        /// <summary>
        /// Handle <see cref="LineWaterEvents.DidCreate"/> event
        /// </summary>
        public void HandleDidCreateLineWater(DidCreateLineWaterArgs arguments)
        {
            // Obtain parameters necessary for the position set
            var gameObjectPosition = arguments.GameObject.transform.position;

            // Move up the road to avoid z-clipping with regions
            arguments.GameObject.transform.position = gameObjectPosition + Vector3.up;
        }

        /// <summary>
        /// Handle <see cref="SegmentEvents.DidCreate"/> event to edit created segments game objects
        /// </summary>
        public void OnDidCreateSegmentCallback(DidCreateSegmentArgs arguments)
        {
            // Obtain parameters necessary for the position set
            var gameObjectPosition = arguments.GameObject.transform.position;

            // Move up the road to avoid z-clipping with regions
            arguments.GameObject.transform.position = gameObjectPosition + Vector3.up * 2;
            
            // Construct exact road label position in 3D space
            var roadLabelPosition = new Vector3(gameObjectPosition.x, 0, gameObjectPosition.z);
            
            // Generate road label
            var roadLabel = _roadLabeller.Create(roadLabelPosition, arguments.MapFeature.Metadata.Name);

            if (roadLabel == null) return;
            
            // Set its road lint to position the label
            roadLabel.SetLine(arguments.GameObject.transform.position, arguments.MapFeature.Shape.Lines[0]);
        }

        /// <summary>
        /// Handle <see cref="AreaWaterEvents.WillCreate"/> event by specifying the area water styles.
        /// </summary>
        public void HandleWillCreateAreaWater(WillCreateAreaWaterArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.AreaWaterStyle;
        }

        /// <summary>
        /// Handle <see cref="ExtrudedStructureEvents.WillCreate"/> event by specifying the extruded structure styles.
        /// </summary>
        public void HandleWillCreateExtrudedStructure(WillCreateExtrudedStructureArgs arguments)
        {
            var isGutBuilding = GutBuildingData.CheckIsGutBuilding(arguments.MapFeature.Metadata.PlaceId);

            if (isGutBuilding)
            {
                var mapGutStyle = MapStyleProvider.Instance.GetMapStyle(MapStyleData.Type.Gut);
                arguments.Style = mapGutStyle.ExtrudedStructureStyle;
            }
            else
            {
                arguments.Style = _mapDefaultStyle.ExtrudedStructureStyle;
            }
        }

        /// <summary>
        /// Handle <see cref="IntersectionEvents.WillCreate"/> event by specifying the intersection styles.
        /// </summary>
        public void HandleWillCreateIntersection(WillCreateIntersectionArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.SegmentStyle;
        }

        /// <summary>
        /// Handle <see cref="LineWaterEvents.WillCreate"/> event by specifying the line water styles.
        /// </summary>
        public void HandleWillCreateLineWater(WillCreateLineWaterArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.LineWaterStyle;
        }

        /// <summary>
        /// Handle <see cref="ModeledStructureEvents.WillCreate"/> event by specifying the modeled structure styles.
        /// </summary>
        public void HandleWillCreateModeledStructure(WillCreateModeledStructureArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.ModeledStructureStyle;
        }

        /// <summary>
        /// Handle <see cref="RegionEvents.WillCreate"/> event by specifying the region styles.
        /// </summary>
        public void HandleWillCreateRegion(WillCreateRegionArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.RegionStyle;
        }

        /// <summary>
        /// Handle <see cref="SegmentEvents.WillCreate"/> event by specifying the segment styles.
        /// </summary>
        public void HandleWillCreateSegment(WillCreateSegmentArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.SegmentStyle;
        }

        /// <summary>
        /// Handle <see cref="SegmentEvents.WillCreate"/> event by specifying the segment styles.
        /// </summary>
        public void HandleWillCreateTerrain(WillCreateTerrainArgs arguments)
        {
            arguments.Style = _mapDefaultStyle.TerrainStyle;
        }

        /// <summary>
        /// Coroutine to paint control texture
        /// </summary>
        /// <param name="arguments">Arguments from painting event</param>
        private IEnumerator PaintControlTexture(AlphaMapsNeedPaintArgs arguments)
        {
            // Get a temporary render texture to be painted onto
            var destinationTexture = arguments.Terrain.terrainData.GetAlphamapTexture(0);
            var renderTextureDescriptor = new RenderTextureDescriptor(destinationTexture.width,
                destinationTexture.height, RenderTextureFormat.ARGB32, 0);
            var temporaryRenderTexture = RenderTexture.GetTemporary(renderTextureDescriptor);
            arguments.RegisterFinalizer(() => RenderTexture.ReleaseTemporary(temporaryRenderTexture));

            yield return null;
            
            // Make the active render texture our temporary one, storing the current one for restoration once we're done
            var renderTexture = RenderTexture.active;
            RenderTexture.active = temporaryRenderTexture;

            // Paint the control texture from the feature mask using the configured material/shader
            Graphics.Blit(arguments.FeatureMaskRenderTexture, temporaryRenderTexture,
                MapStyleProvider.Instance.TerrainControlTexture);

            destinationTexture.ReadPixels(new Rect(0, 0, destinationTexture.width, destinationTexture.height), 0, 0);
            destinationTexture.Apply();
            
            arguments.Terrain.terrainData.SetBaseMapDirty();

            // Restore the previously active render texture
            RenderTexture.active = renderTexture;
        }
    }
}