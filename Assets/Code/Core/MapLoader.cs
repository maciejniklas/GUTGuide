using System;
using Google.Maps;
using Google.Maps.Coord;
using GUTGuide.DataStructures;
using GUTGuide.UI;
using GUTGuide.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace GUTGuide.Core
{
    /// <summary>
    /// Responsible for map objects loading and unloading
    /// </summary>
    [RequireComponent(typeof(MapsService))]
    public class MapLoader : MonoBehaviour
    {
        [Tooltip("Global coordinates of Gdańsk University of Technology")]
        [SerializeField] private LatLng gutCoordinates = new LatLng(54.3714279, 18.6169474);
        [Tooltip("Distance in which map objects will be rendered")]
        [SerializeField] [Min(10)] private float renderDistance = 1000f;
        
        /// <summary>
        /// Has the application quit?
        /// </summary>
        public bool HasQuit { get; private set; }
        /// <summary>
        /// Was the map initialized?
        /// </summary>
        public bool IsInitialized { get; private set; }
        /// <summary>
        /// Is the map currently loading?
        /// </summary>
        public bool IsLoading { get; private set; }

        /// <summary>
        /// Event called just before map starts loading
        /// </summary>
        public UnityEvent OnMapLoadingStarted;

        /// <summary>
        /// Reference to the map service
        /// </summary>
        private MapsService _mapsService;
        /// <summary>
        /// Reference to the main camera of the game
        /// </summary>
        private Camera _mainCamera;
        /// <summary>
        /// Default map objects styling
        /// </summary>
        private GameObjectOptions _mapDefaultStyle;

        private void Awake()
        {
            _mapsService = GetComponent<MapsService>();
            IsLoading = true;
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            // Define the origin point of the loaded map
            _mapsService.InitFloatingOrigin(gutCoordinates);

            // Obtain default map objects styling
            _mapDefaultStyle = MapStyleProvider.Instance.GetMapStyle(MapStyleData.Type.Default);
            
            // Initialize events listeners
            _mapsService.Events.MapEvents.Loaded.AddListener((_) => IsLoading = false);
            
            // Initialize error handler
            _mapsService.Events.MapEvents.LoadError.AddListener(ErrorHandler.MapLoadingError);

            IsInitialized = true;
            LoadMap();
        }

        private void OnApplicationQuit()
        {
            HasQuit = true;
        }

        private void OnDestroy()
        {
            OnMapLoadingStarted.RemoveAllListeners();
        }

        /// <summary>
        /// Loads objects in the camera viewport
        /// </summary>
        private void LoadMap()
        {
            // If app is closed do nothing
            if (HasQuit) return;

            // Initial steps
            IsLoading = true;
            OnMapLoadingStarted?.Invoke();

            // Load objects in viewport
            _mapsService.MakeMapLoadRegion().AddViewport(_mainCamera, renderDistance).Load(_mapDefaultStyle);
        }
    }
}