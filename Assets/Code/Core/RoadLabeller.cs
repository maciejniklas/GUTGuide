using System.Collections.Generic;
using GUTGuide.UI;
using UnityEngine;

namespace GUTGuide.Core
{
    /// <summary>
    /// Component for adding labels to show the names of the roads
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class RoadLabeller : MonoBehaviour
    {
        [Tooltip("Template prefab for road labels instantiating")]
        [SerializeField] private RoadLabel roadLabelPrefab;

        /// <summary>
        /// All created object labels, stored by a key
        /// </summary>
        private Dictionary<string, RoadLabel> _roadLabelsByKey;
        /// <summary>
        /// Reference to the <see cref="MapOriginUpdater"/>
        /// </summary>
        private MapOriginUpdater _mapOriginUpdater;
        /// <summary>
        /// Reference to the <see cref="MixedMapLoader"/>
        /// </summary>
        private MixedMapLoader _mixedMapLoader;
        /// <summary>
        /// Is the road labeller initialized?
        /// </summary>
        private bool _isInitialized;

        private void Awake()
        {
            var mainCamera = Camera.main;
            
            _roadLabelsByKey = new Dictionary<string, RoadLabel>();
            
            if (mainCamera is null) return;
            
            _mapOriginUpdater = mainCamera.GetComponent<MapOriginUpdater>();
            _mixedMapLoader = mainCamera.GetComponent<MixedMapLoader>();
        }

        private void Start()
        {
            _isInitialized = true;
        }

        private void OnDisable()
        {
            if (_mapOriginUpdater != null)
                _mapOriginUpdater.onMapOriginUpdate.RemoveListener(OnMapOriginUpdateCallback);
            if (_mixedMapLoader != null) _mixedMapLoader.onMapRegionUnload.RemoveListener(OnRegionUnloadedCallback);
            Clear();
        }

        private void OnEnable()
        {
            _mapOriginUpdater.onMapOriginUpdate.AddListener(OnMapOriginUpdateCallback);
            _mixedMapLoader.onMapRegionUnload.AddListener(OnRegionUnloadedCallback);
            
            if (_isInitialized) _mixedMapLoader.Reload();
        }

        /// <summary>
        /// Deletes all road name tags from the scene
        /// </summary>
        private void Clear()
        {
            _roadLabelsByKey.Clear();

            foreach (Transform child in transform)
            {
                Destroy(child.gameObject);
            }
        }

        /// <summary>
        /// Generate new instance of the road label
        /// </summary>
        /// <param name="initialPosition">Position of the label in 3D space</param>
        /// <param name="roadKey">Id of the road</param>
        /// <param name="roadName">Name of the road that will be visible in the scene</param>
        /// <returns></returns>
        public RoadLabel Create(Vector3 initialPosition, string roadName)
        {
            // If the name is empty do not create a new label
            if (string.IsNullOrWhiteSpace(roadName)) return null;

            RoadLabel roadLabel;

            // Check if the label already exists in the road labels pool or create a new one if not
            if (_roadLabelsByKey.ContainsKey(roadName))
            {
                roadLabel = _roadLabelsByKey[roadName];
            }
            else
            {
                roadLabel = Instantiate(roadLabelPrefab, transform);
                roadLabel.SetText(roadName);
                
                _roadLabelsByKey.Add(roadName, roadLabel);
            }

            // Set the position of the road label
            roadLabel.transform.position = initialPosition;

            return roadLabel;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="offset"></param>
        private void OnMapOriginUpdateCallback(Vector3 offset)
        {
            foreach (Transform child in transform) child.position += offset;
        }

        private void OnRegionUnloadedCallback(Vector3 center, float radius)
        {
            var keysToUnload = new List<string>();

            // Collect all road dictionary keys that labels need to hide
            foreach (var key in _roadLabelsByKey.Keys)
            {
                var roadLabelPosition = _roadLabelsByKey[key].transform.position;
                var distance = Vector3.Distance(roadLabelPosition, center);
                
                if (distance > radius) keysToUnload.Add(key);
            }
    
            // Remove road labels
            foreach (var key in keysToUnload)
            {
                Destroy(_roadLabelsByKey[key].gameObject);
                _roadLabelsByKey.Remove(key);
            }
        }
    }
}