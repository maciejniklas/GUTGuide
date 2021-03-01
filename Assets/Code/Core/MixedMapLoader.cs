﻿using Google.Maps;
using Google.Maps.Coord;
using Google.Maps.Loading;
using GUTGuide.DataStructures;
using GUTGuide.Utilities;
using UnityEngine;

namespace GUTGuide.Core
{
    /// <summary>
    /// Responsible for map objects loading and unloading
    /// </summary>
    [RequireComponent(typeof(MapLoader))]
    [RequireComponent(typeof(MixedZoom))]
    public class MixedMapLoader : MonoBehaviour
    {
        [Header("General settings")]
        
        [Tooltip("Global coordinates of Gdańsk University of Technology")]
        [SerializeField] private LatLng gutCoordinates = new LatLng(54.3714279,18.6169474);
        [Tooltip("Load the map from the current camera position when it has moved this far")]
        [SerializeField] private float loadDistance = 10f;
        [Tooltip("Angle after which map objects will be updated")]
        [SerializeField] [Range(1, 90)] private float updateAngle = 5f;
        [Tooltip("Unload unused parts of the map when the camera position has moved this far")]
        [SerializeField] private float unloadDistance = 100f;

        [Header("References")]
        
        [SerializeField] private Transform groundPlaneTransform;

        /// <summary>
        /// Reference to the main camera of the game
        /// </summary>
        private Camera _mainCamera;
        /// <summary>
        /// Default map objects styling
        /// </summary>
        private GameObjectOptions _mapDefaultStyle;
        /// <summary>
        /// Camera position when <see cref="MapLoader.Load()"/> was last called
        /// </summary>
        private Vector3? _lastLoadPosition;
        /// <summary>
        /// Rotation of the camera the last time the map was loaded
        /// </summary>
        private Quaternion? _lastLoadAngle;
        /// <summary>
        /// Camera position when <see cref="MapLoader.UnloadUnused()"/> was last called
        /// </summary>
        private Vector3? _lastUnloadPosition;
        /// <summary>
        /// Reference to the <see cref="MapLoader"/>
        /// </summary>
        private MapLoader _mapLoader;

        private void Awake()
        {
            _mainCamera = Camera.main;
            _mapLoader = GetComponent<MapLoader>();
        }

        private void Start()
        {
            // Obtain default map objects styling
            _mapDefaultStyle = MapStyleProvider.Instance.GetMapStyle(MapStyleData.Type.Default);

            _mapLoader.Init(_mapDefaultStyle);
            _mapLoader.MapsService.InitFloatingOrigin(gutCoordinates);
        }

        private void Update()
        {
            // Get reference of the main camera position
            var mainCameraTransform = _mainCamera.transform;
            var mainCameraPosition = mainCameraTransform.position;
            var mainCameraRotation = mainCameraTransform.rotation;
            
            // Update ground plane position
            groundPlaneTransform.position = new Vector3(mainCameraPosition.x, groundPlaneTransform.position.y,
                mainCameraPosition.z);
            
            // Load the map with mixed zoom, centered on the current camera location
            if (_lastLoadAngle == null || Quaternion.Angle(mainCameraRotation, _lastLoadAngle.Value) >=
                updateAngle)
            {
                _mapLoader.Load();
                _lastLoadAngle = mainCameraRotation;
            }
            
            // Load the map with mixed zoom, centered on the current camera location
            if (_lastLoadPosition == null || (mainCameraPosition - _lastLoadPosition.Value).sqrMagnitude >=
                Mathf.Pow(loadDistance, 2))
            {
                _mapLoader.Load();
                _lastLoadPosition = mainCameraPosition;
            }

            //Unload map GameObjects that have been inactive for longer than MixedMapLoader.UnloadUnusedSeconds
            if (_lastUnloadPosition == null || (mainCameraPosition - _lastUnloadPosition.Value).sqrMagnitude >=
                Mathf.Pow(unloadDistance, 2))
            {
                _mapLoader.UnloadUnused();
                _lastUnloadPosition = mainCameraPosition;
            }
        }
    }
}